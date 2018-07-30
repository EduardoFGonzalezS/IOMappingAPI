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
            PLCs = new PLC_Repository(context);
            Attributes = new Attribute_Repository(context);
            PLCTags = new PLCTag_Repository(context, PLCs);
            IOTags = new IOTag_Repository(context);
            Contents = new Content_Repository(context);
            Instances = new Instance_Repository(context);
            
        }

        /// <summary>
        /// Evaluates a collection of records, and takes care of the whole CRUD operation.
        /// Meaning, it checks existance in database, and if not found, its created in database.
        /// If it exists in database with different info, it updates the database.
        /// In other words, "syncs" the received collection of contents to the database.
        /// /// </summary>
        /// <param name="_Content">A list of instance content to be added / updated to the database ///</param>
        public async Task<HttpResponseMessage> PushRecordsToDbset(List<InstanceContent> _Contents)
        {
            //01 - Initial Response
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            //02 - Check all PLC entities passed in argument, and push them to the database
            List<PLC> PLCs_FromContents = _Contents.Select(c => c.IOTag.PLC)
                                .Union(_Contents.Select(c => c.PLCTag.PLC))
                                .Where(vPLC => vPLC != null)
                                .GroupBy(vPLC => vPLC.Name)
                                .Select(vPLC => vPLC.First()).ToList();
            List<PLC> PLCs_ToPush = PLCs.GetListSyncFromDB(PLCs_FromContents);

            PLCs.PushToDbset(PLCs_ToPush);

            //03 - SAVE THE DB CONTEXT ########  
            context.SaveChanges();

            //04 - Extract the individual entities (PLCTags, Instances, etc.) from the collection. 
            //These objects will be pushed to the database. In case of missing Null Info, try to update it from the database
            List<Attribute> Attributes_ToPush = _Contents.Select(c => c.Attribute).ToList();
            List<Instance> Instances_ToPush = _Contents.Select(c => c.Instance).ToList();
            List<IOTag> IOTags_FromContents = _Contents.Select(c => c.IOTag).ToList();
            List<IOTag> IOTags_ToPush = IOTag_UpdatedPLCIDs(IOTags_FromContents);


            List<PLCTag> PLCTags_FromContents = _Contents.Select(c => c.PLCTag).ToList();
            List<PLCTag> PLCTags_ToPush = PLCTags.GetListSyncFromDB(PLCTags_FromContents);

            //05 - Push individual entities to Database.
            Attributes.PushToDbset(Attributes_ToPush);
            Instances.PushToDbset(Instances_ToPush);
            IOTags.PushToDbset(IOTags_ToPush);
            PLCTags.PushToDbset(PLCTags_ToPush);

            //06 - SAVE THE DB CONTEXT ########  
            context.SaveChanges();

            //07 - Update the Primary Keys on all entities on the contents collection. This info is extracted from the database.
            // After that, push to database.
            List<InstanceContent> PContents = FetchIds(_Contents);
            Contents.PushToDbset(PContents);

            //09 - SAVE THE DB CONTEXT ########
            context.SaveChanges();

            //EXTRA - For future use
            //List<InstanceContent> Surplus = Contents.SurplusInDatabase(PContents);
            //Contents.DeleteList(Surplus);
            //context.SaveChanges();

            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }

        private List<InstanceContent> FetchIds(List<InstanceContent> _ContentList)
        {
            List<InstanceContent> ReturnList = (from Cont in _ContentList
                                                join db in Contents.EntityCollection
                                                on
                                                new { InstanceName = Cont.Instance.Name, AttributeName = Cont.Attribute.Name, IOTagName = Cont.IOTag.Name, PLCTagName = Cont.PLCTag.Name }
                                                equals
                                                new { InstanceName = db.Instance.Name, AttributeName = db.Attribute.Name, IOTagName = db.IOTag.Name, PLCTagName = db.PLCTag.Name }
                                                into JoinedTbl
                                                from db in JoinedTbl.DefaultIfEmpty(
                                                    new InstanceContent()
                                                            {
                                                                InstanceContentID = Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name),
                                                                InstanceID = Instances.GetID(Cont.Instance.Name),
                                                                Instance = new Instance { Name = Cont.Instance.Name, ID = Instances.GetID(Cont.Instance.Name) },
                                                                AttributeID = Attributes.GetID(Cont.Attribute.Name),
                                                                Attribute = new Attribute { Name = Cont.Attribute.Name, ID = Attributes.GetID(Cont.Attribute.Name) },
                                                                IOTagID = IOTags.GetID(Cont.IOTag.Name),
                                                                IOTag = new IOTag { Name = Cont.IOTag.Name, ID = IOTags.GetID(Cont.IOTag.Name) },
                                                                PLCTagID = PLCTags.GetID(Cont.PLCTag.Name),
                                                                PLCTag = new PLCTag { Name = Cont.PLCTag.Name, ID = PLCTags.GetID(Cont.PLCTag.Name) }
                                                            }
                                                    )
                                                select new InstanceContent
                                                {
                                                    InstanceContentID = db != null ? db.InstanceContentID : 0,
                                                    InstanceID = db != null ? db.InstanceID : 0,
                                                    Instance = db != null ? db.Instance: new Instance { Name = Cont.Instance.Name, ID = 0 },
                                                    AttributeID = db != null ? db.AttributeID : 0,
                                                    Attribute = db != null ? db.Attribute : new Attribute { Name = Cont.Attribute.Name, ID = 0 },
                                                    IOTagID = db != null ? db.IOTagID : 0,
                                                    IOTag = db != null ? db.IOTag : new IOTag { Name = Cont.IOTag.Name, ID = 0, PLCID = 0, PLC = new PLC() },
                                                    PLCTagID = db != null ? db.PLCTagID : 0,
                                                    PLCTag = db != null ? db.PLCTag : new PLCTag { Name = Cont.PLCTag.Name, ID = 0,
                                                        Rack = Cont.PLCTag.Rack, Slot = Cont.PLCTag.Slot, Point = Cont.PLCTag.Point, PLCID = 0, PLC = new PLC() }
                                                }).ToList();
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
                                           PLC = db != null ? db.PLC : new PLC (),
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
                                   PLC = db != null ? db.PLC : new PLC { Name = ents.PLC.Name }
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