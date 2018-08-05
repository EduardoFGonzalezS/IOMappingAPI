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
        public IContent_Repository Contents { get; private set; }
        public IPLC_Repository PLCs { get; private set; }

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The object context</param>
        public GalaxyObject_UoW(GalaxyObjectContext ctx , IContent_Repository _conts)
        {
            context = ctx;
            PLCs = new PLC_Repository(context);
            Contents = _conts; Contents.context = ctx;
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
            List<PLC> PLCs_ToPush = _Contents.Select(c => c.IOTag.PLC)
                                .Union(_Contents.Select(c => c.PLCTag.PLC))
                                .Where(vPLC => vPLC != null)
                                .GroupBy(vPLC => vPLC.Name)
                                .Select(vPLC => vPLC.First()).ToList();

            PLCs.PushToDbset(PLCs_ToPush);

            //03 - SAVE THE DB CONTEXT ########  
            context.SaveChanges();

            //04 - Extract the individual entities (PLCTags, Instances, etc.) from the collection. 
            //These objects will be pushed to the database. In case of missing Null Info, try to update it from the database
            List<Attribute> Attributes_ToPush = _Contents.Select(c => c.Attribute).ToList();
            List<Instance> Instances_ToPush = _Contents.Select(c => c.Instance).ToList();
            List<IOTag> IOTags_ToPush = _Contents.Select(c => c.IOTag).ToList();
            List<PLCTag> PLCTags_ToPush = _Contents.Select(c => c.PLCTag).ToList();

            //05 - Push individual entities to Database.
            Contents.Attributes.PushToDbset(Attributes_ToPush);
            Contents.Instances.PushToDbset(Instances_ToPush);
            Contents.IOTags.PushToDbset(IOTags_ToPush);
            Contents.PLCTags.PushToDbset(PLCTags_ToPush);

            //06 - SAVE THE DB CONTEXT ########  
            context.SaveChanges();

            //07 - Update the Primary Keys on all entities on the contents collection. This info is extracted from the database.
            // After that, push to database.
            List<InstanceContent> PContents = Contents.GetListSyncFromDB(_Contents);
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