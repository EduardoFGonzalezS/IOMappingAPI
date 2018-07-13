using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IGalaxyObject_UoW : IDisposable
    {
        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        int Commit();

        IAttribute_Repository Attributes { get; }
        IIOTag_Repository IOTags { get; }
        IPLCTag_Repository PLCTags { get; }
        IInstance_Repository Instances { get; }
        IContent_Repository Contents { get; }
        void PushRecordsToDbset(List<InstanceContent> Contents_ToPush);
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


        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The object context</param>
        public GalaxyObject_UoW(GalaxyObjectContext ctx)
        {
            context = ctx;
            Attributes = new Attribute_Repository(context);
            IOTags = new IOTag_Repository(context);
            PLCTags = new PLCTag_Repository(context);
            IOTags = new IOTag_Repository(context);
            Contents = new Content_Repository(context);
            Instances = new Instance_Repository(context);
        }

        public void PushRecordsToDbset(List<InstanceContent> Passed_Contents)
        {
            List<Attribute> Attributes_ToPush = Passed_Contents.Select(c => c.Attribute).ToList();
            List<Instance> Instances_ToPush = Passed_Contents.Select(c => c.Instance).ToList();
            List<IOTag> IOTags_ToPush = Passed_Contents.Select(c => c.IOTag).ToList();
            List<PLCTag> PLCTags_ToPush = Passed_Contents.Select(c => c.PLCTag).ToList();

            Attributes.PushToDbset(Attributes_ToPush);
            Instances.PushToDbset(Instances_ToPush);
            IOTags.PushToDbset(IOTags_ToPush);
            PLCTags.PushToDbset(PLCTags_ToPush);

            List<InstanceContent> PContents = Passed_Contents;
            int FoundId = 0;
            Commit();

            foreach (InstanceContent ic in PContents)
            {
                FoundId = Attributes.GetID(ic.Attribute.Name);
                ic.AttributeID = FoundId; ic.Attribute.ID = FoundId;

                FoundId = Instances.GetID(ic.Instance.Name);
                ic.InstanceID = FoundId; ic.Instance.ID = FoundId;

                FoundId = IOTags.GetID(ic.IOTag.Name);
                ic.IOTagID = FoundId; ic.IOTag.ID = FoundId;

                FoundId = PLCTags.GetID(ic.PLCTag.Name);
                ic.PLCTagID = FoundId; ic.PLCTag.ID = FoundId;
            }

            Contents.PushToDbset(PContents);
            Commit();

            List<InstanceContent> Surplus = Contents.SurplusInDatabase(PContents);
            Contents.DeleteList(Surplus);

            Commit();
        }

        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public int Commit()
        {
            // Save changes with the default options
            return context.SaveChanges();
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