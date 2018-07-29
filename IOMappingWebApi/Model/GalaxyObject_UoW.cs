using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Diagnostics;

namespace IOMappingWebApi.Model
{
    public interface IGalaxyObject_UoW : IDisposable
    {
        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        Task Commit();

        IAttribute_Repository Attributes { get; }
        IIOTag_Repository IOTags { get; }
        IPLCTag_Repository PLCTags { get; }
        IInstance_Repository Instances { get; }
        IContent_Repository Contents { get; }
        IPLC_Repository PLCs { get; }
        Task<HttpResponseMessage> PushRecordsToDbset(List<InstanceContent> Contents_ToPush);
    }

    /// <summary>
    /// The Entity Framework implementation of IUnitOfWork
    /// </summary>
    public sealed class GalaxyObject_UoW : IGalaxyObject_UoW
    {
        /// <summary>
        /// The DbContext
        /// </summary>
        private GalaxyObjectContext context;

        public IAttribute_Repository Attributes { get; private set; }
        public IIOTag_Repository IOTags { get; private set; }
        public IPLCTag_Repository PLCTags { get; private set; }
        public IInstance_Repository Instances { get; private set; }
        public IContent_Repository Contents { get; private set; }
        public IPLC_Repository PLCs { get; private set; }

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The object context</param>
        public GalaxyObject_UoW(GalaxyObjectContext ctx)
        {
            context = ctx;
            Attributes = new Attribute_Repository(context);
            PLCTags = new PLCTag_Repository(context);
            IOTags = new IOTag_Repository(context);
            Contents = new Content_Repository(context);
            Instances = new Instance_Repository(context);
            PLCs = new PLC_Repository(context);
        }

        public async Task<HttpResponseMessage> PushRecordsToDbset(List<InstanceContent> _Contents)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";


            List<PLC> PLCs1 = _Contents.Select(c => c.IOTag.PLC).ToList();
            List<PLC> PLCs2 = _Contents.Select(c => c.PLCTag.PLC).ToList();
            List<PLC> PLCs_ToPush = PLCs1.Union(PLCs2).ToList();
            PLCs.PushToDbset(PLCs_ToPush);

            context.SaveChanges();

            List<Attribute> Attributes_ToPush = _Contents.Select(c => c.Attribute).ToList();
            List<Instance> Instances_ToPush = _Contents.Select(c => c.Instance).ToList();
            List<IOTag> IOTags_FromContents = _Contents.Select(c => c.IOTag).ToList();
            List<IOTag> IOTags_ToPush = IOTag_UpdatedPLCIDs(IOTags_FromContents);

            List<PLCTag> PLCTags_FromContents = _Contents.Select(c => c.PLCTag).ToList();
            List<PLCTag> PLCTags_ToPush = PLCTags_FromContents;// PLCTag_UpdatedPLCIDs(PLCTags_FromContents);

            Attributes.PushToDbset(Attributes_ToPush);
            Instances.PushToDbset(Instances_ToPush);
            IOTags.PushToDbset(IOTags_ToPush);
            PLCTags.PushToDbset(PLCTags_ToPush);

            context.SaveChanges();

            List<InstanceContent> PContents = FetchIds(_Contents);

            Contents.PushToDbset(PContents);
            //context.SaveChanges();

            //List<InstanceContent> Surplus = Contents.SurplusInDatabase(PContents);
            //Contents.DeleteList(Surplus);

            //context.SaveChanges();

            Debug.WriteLine(6666666666); //--------------------------------------------
            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }

        private List<InstanceContent> FetchIds(List<InstanceContent> _ContentList)
        {
            List<InstanceContent> ReturnList = new List<InstanceContent>();
            int f_InstanceID; int f_AttributeID; int f_IOTagID; int f_PLCTagID; int f_ContentID;

            foreach (InstanceContent ic in _ContentList)
            {
                f_InstanceID = Instances.GetID(ic.Instance.Name);
                f_AttributeID = Attributes.GetID(ic.Attribute.Name);
                f_IOTagID = IOTags.GetID(ic.IOTag.Name);
                f_PLCTagID = PLCTags.GetID(ic.PLCTag.Name);
                f_ContentID = Contents.GetID(ic.Instance.Name, ic.Attribute.Name);

                ReturnList.Add(
                    new InstanceContent
                    {
                        InstanceContentID = f_ContentID,
                        InstanceID = f_InstanceID,
                        Instance = new Instance { Name = ic.Instance.Name, ID = f_InstanceID },
                        AttributeID = f_AttributeID,
                        Attribute = new Attribute { Name = ic.Attribute.Name, ID = f_AttributeID },
                        IOTagID = f_IOTagID,
                        IOTag = new IOTag { Name = ic.IOTag.Name, ID = f_IOTagID },
                        PLCTagID = f_PLCTagID,
                        PLCTag = new PLCTag { Name = ic.PLCTag.Name, ID = f_PLCTagID }
                    });
            }
            return ReturnList;
        }

        public List<PLCTag> PLCTag_UpdatedPLCIDs(List<PLCTag> _Entities)
        {
            List<PLCTag> UpdatedList = (from ents in _Entities
                                       join db in PLCTags.EntityCollection on ents.Name equals db.Name
                                       into JoinedTbl
                                       from db in JoinedTbl.DefaultIfEmpty(new PLCTag())
                                       select new PLCTag
                                       {
                                           Name = ents.Name,
                                           ID = ents.ID,
                                           PLCID = db != null ? db.PLC.ID : 0,
                                           PLC = new PLC { Name = ents.PLC.Name, ID = db != null ? db.PLC.ID : 0 },
                                           Rack = ents.Rack,
                                           Slot = ents.Slot,
                                           Point = ents.Point
                                       }
                               ).ToList();

            return UpdatedList;
        }

        public List<IOTag> IOTag_UpdatedPLCIDs(List<IOTag> _Entities)
        { 
            List<IOTag> UpdatedList = (from ents in _Entities
                               join db in IOTags.EntityCollection on ents.Name equals db.Name
                               into JoinedTbl
                               from db in JoinedTbl.DefaultIfEmpty(new IOTag())
                               select new IOTag
                               {
                                   Name = ents.Name,
                                   ID = ents.ID,
                                   PLCID = db != null ? db.PLC.ID : 0 ,
                                   PLC = new PLC{Name = ents.PLC.Name, ID = db != null ? db.PLC.ID : 0 }
                               }
                               ).ToList();

            return UpdatedList;
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        /// 
        public async Task Commit()
        {
            using (var ctx = context)
            {
                await ctx.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all external resources.
        /// </summary>
        /// <param name="disposing">The dispose indicator.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (context != null)
                {
                    context.Dispose();
                    context = null;
                }
            }
        }
    }
}