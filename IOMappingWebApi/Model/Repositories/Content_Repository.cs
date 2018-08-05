using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace IOMappingWebApi.Model
{
    public interface IContent_Repository
    {
        List<InstanceContent> EntityCollection { get; set; }
        void Insert(InstanceContent Entity);
        void PushToDbset(List<InstanceContent> Entities);
        int GetID(String InstanceName, String AttributeName);
        List<InstanceContent> SurplusInDatabase(List<InstanceContent> _Contents);
        void DeleteList(List<InstanceContent> Entities);
        List<InstanceContent> GetListSyncFromDB(List<InstanceContent> _Entities);
        GalaxyObjectContext context { get; set; }

        IAttribute_Repository Attributes { get; }
        IIOTag_Repository IOTags { get; }
        IPLCTag_Repository PLCTags { get; }
        IInstance_Repository Instances { get; }
    }

    public class Content_Repository : IContent_Repository
    {
        // #01 - Fields
        public IInstance_Repository Instances { get; private set; }
        public IAttribute_Repository Attributes { get; private set; }
        public IIOTag_Repository IOTags { get; private set; }
        public IPLCTag_Repository PLCTags { get; private set; }

        public GalaxyObjectContext context { get; set; }

        // #02 - Constructos
        public Content_Repository(GalaxyObjectContext ctx
            , IInstance_Repository _Instances
            , IAttribute_Repository _Attributes
            , IIOTag_Repository _IOTags
            , IPLCTag_Repository _PLCTags)
        {
            Instances = _Instances;
            Attributes = _Attributes;
            IOTags = _IOTags;
            PLCTags = _PLCTags;
        }

        // #03 - Properties
        public List<InstanceContent> EntityCollection
        {
            get
            {
            var IContents = context.InstanceContent                        
                .Include(c => c.Instance)
                .Include(c => c.Attribute)
                .Include(c => c.IOTag)
                .Include(c => c.IOTag.PLC)
                .Include(c => c.PLCTag)
                .Include(c => c.PLCTag.PLC).ToList();
             return IContents;
            }

            set
            {
                List<InstanceContent> ContentList = value;
                PushToDbset(ContentList);
            }
        }

        // #04 - Methods (Single Operations)
        public void Insert(InstanceContent Entity)
        {
            context.Set<InstanceContent>().Add(Entity);
        }
        public void Delete(InstanceContent Entity)
        {
            context.Set<InstanceContent>().Remove(Entity);
        }
        public void Update(int id, InstanceContent Entity)
        {
            using (var ctx = context)
            {
                var FoundEntity = ctx.Set<InstanceContent>().Find(id);

                if (FoundEntity != null)
                { ctx.Entry(FoundEntity).CurrentValues.SetValues(Entity); }

                ctx.SaveChanges();
            }
        }

        public int GetID(String InstanceName, String AttributeName)
        {
            var FoundEntity = context.Set<InstanceContent>()
                .FirstOrDefault(e => e.Instance.Name == InstanceName && e.Attribute.Name == AttributeName);

            if (FoundEntity != null)
            { return FoundEntity.InstanceContentID; }
            else
            { return 0; }   
        }

        // #05 - Methods (BULK Operations)
        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are found on the Database</returns>  
        public List<InstanceContent> InDatabase(List<InstanceContent> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<InstanceContent>() on
                           new { AttributeID = ents.AttributeID, InstanceID = ents.InstanceID }
                           equals
                           new { AttributeID = db.AttributeID, InstanceID = db.InstanceID }
                           select new
                           {
                               db.InstanceContentID,
                               db.InstanceID,db.Instance,
                               db.AttributeID,db.Attribute,
                               ents.IOTagID,
                               ents.PLCTagID,
                           })
                        .ToList().Select(e => new InstanceContent
                        {
                            InstanceContentID = e.InstanceContentID,
                            InstanceID = e.InstanceID,Instance = e.Instance,
                            AttributeID = e.AttributeID, Attribute = e.Attribute,

                            IOTagID = e.IOTagID, PLCTagID = e.PLCTagID,
                        }).ToList();


            return (List<InstanceContent>)Results;
        }

        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are NOT found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are NOT found on the Database</returns>  
        public List<InstanceContent> NOTInDatabase(List<InstanceContent> Entities)
        {

            var Results = (from ents in Entities
                           join db in context.InstanceContent on
                           new {AttributeID = ents.AttributeID , InstanceID = ents.InstanceID}
                           equals
                           new { AttributeID = db.AttributeID, InstanceID = db.InstanceID }
                           into DbResults
                           where !DbResults.Any()
                           select ents).ToList();
            int a = 0;
            return (List<InstanceContent>)Results;
        }

        public List<InstanceContent> SurplusInDatabase(List<InstanceContent> _Contents)
        {
            String InstanceName = _Contents.Select(c => c.Instance.Name).FirstOrDefault();
            var Surplus = EntityCollection
                .Where(conts => !_Contents.Any(ec => (
                ec.Attribute.ID == conts.Attribute.ID 
                && ec.Instance.ID == conts.Instance.ID
                && ec.IOTag.ID == conts.IOTag.ID 
                && ec.PLCTag.ID == conts.PLCTag.ID))
                && conts.Instance.Name == InstanceName);

            return Surplus.ToList();
        }

        /// <summary>
        /// Evaluates a collection of entities, and takes care of the whole CRUD operation.
        /// Meaning, it checks existance in database, and if not found, its created in database.
        /// If it exists in database with different info, it updates the database.
        /// In other words, "syncs" the received collection of entities to the database.
        /// /// </summary>
        /// <param name="Entities">A list of Entities to be added / updated to the database ///</param>
        public void PushToDbset(List<InstanceContent> Entities)
        {
            //Extract Contained Classes, and push them to the database
            List<Attribute> Attributes_ToPush = Entities.Select(c => c.Attribute).ToList();
            List<Instance> Instances_ToPush = Entities.Select(c => c.Instance).ToList();
            List<IOTag> IOTags_ToPush = Entities.Select(c => c.IOTag).ToList();

            List<PLCTag> PLCTags_ToPush = Entities.Select(c => c.PLCTag).ToList();

            Attributes.PushToDbset(Attributes_ToPush);
            Instances.PushToDbset(Instances_ToPush);
            IOTags.PushToDbset(IOTags_ToPush);
            PLCTags.PushToDbset(PLCTags_ToPush);
            context.SaveChanges();

            //Update the contained classes in the entities, so that they are not null or incomplete
            List<InstanceContent> EntsToPush = GetListSyncFromDB(Entities);

            //Find contents that as NOT in the database, and insert them
            List<InstanceContent> Entities_NOTinDb = NOTInDatabase(EntsToPush);
            if (Entities_NOTinDb.Count
                > 0) { InsertList(Entities_NOTinDb); }

            //Find contents that are in the database, and update them
            List<InstanceContent> Entities_inDb = InDatabase(EntsToPush);
            if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
        }

        /// <summary>
        /// Inserts (Bulk operation) a whole list of entities into the database
        /// /// </summary>
        /// <param name="Entities">A list of instance content to be inserted to the database ///</param>
        public void InsertList(List<InstanceContent> Entities)
        {
            foreach (InstanceContent Entity in Entities)
            {
                if (Entity != null)
                { context.Set<InstanceContent>().Add(Entity); }
            }
        }

        /// <summary>
        /// Updates (Bulk operation) a whole list of entities in the database
        /// /// </summary>
        /// <param name="Entities">A list of instance content to be updated in the database ///</param>
        public void UpdateList(List<InstanceContent> Entities)
        {
            foreach (InstanceContent Entity in Entities)
            {
                int id = Entity.InstanceContentID;
                var FoundEntity = context.Set<InstanceContent>().Find(id);

                if (FoundEntity != null && FoundEntity != Entity)
                { context.Entry(FoundEntity).CurrentValues.SetValues(Entity); }
            }
        }

        public List<InstanceContent> GetListSyncFromDB(List<InstanceContent> _Entities)
        {
            List<InstanceContent> UpdatedList = (from Cont in _Entities
                                                select new InstanceContent()
                                                {
                                                    InstanceContentID = GetID(Cont.Instance.Name, Cont.Attribute.Name),
                                                    Instance = Instances.GetSyncFromDB(Cont.Instance),
                                                    InstanceID = Instances.GetID(Cont.Instance.Name),
                                                    Attribute = Attributes.GetSyncFromDB(Cont.Attribute),
                                                    AttributeID = Attributes.GetID(Cont.Attribute.Name),
                                                    PLCTag = Cont.PLCTag != null ? PLCTags.GetSyncFromDB(Cont.PLCTag) : null,
                                                    PLCTagID = Cont.PLCTag != null ? PLCTags.GetID(Cont.PLCTag.Name) : Cont.PLCTagID,
                                                    IOTag = IOTags.GetSyncFromDB(Cont.IOTag),
                                                    IOTagID = IOTags.GetID(Cont.IOTag.Name)
                                                }).ToList();
            return UpdatedList;
        }

        public void DeleteList(List<InstanceContent> Entities)
        {
            foreach (InstanceContent Entity in Entities)
            {
                Delete(Entity);
            }
        }
    }
}