using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace IOMappingWebApi.Model
{
    public interface IContent_Repository //: IRecordRepository<InstanceContent>
    {
        List<InstanceContent> EntityCollection { get; set; }
        void Insert(InstanceContent Entity);
        void PushToDbset(List<InstanceContent> Entities);
        int GetID(String InstanceName, String AttributeName);
        List<InstanceContent> SurplusInDatabase(List<InstanceContent> _Contents);
        void DeleteList(List<InstanceContent> Entities);
    }

    public class Content_Repository : IContent_Repository
    {
        // #01 - Fields
        private GalaxyObjectContext context;

        // #02 - Constructors
        public Content_Repository(GalaxyObjectContext ctx)
        {
            context = ctx;
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
            using (context)
            {
                var FoundEntity = context.Set<InstanceContent>().Find(id);
                context.Entry(FoundEntity).CurrentValues.SetValues(Entity);
            }
        }

        public int GetID(String InstanceName, String AttributeName)
        {
            var FoundEntity = context.Set<InstanceContent>()
                .FirstOrDefault(e => e.Instance.Name == InstanceName && e.Attribute.Name == AttributeName);
            return FoundEntity.InstanceContentID;
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
                           join db in context.InstanceContent on
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
        public void PushToDbset(List<InstanceContent> Entities)
        {
            List<InstanceContent> Entities_NOTinDb = NOTInDatabase(Entities);
            InsertList(Entities_NOTinDb);
        }
        public void InsertList(List<InstanceContent> Entities)
        {
            foreach (InstanceContent Entity in Entities)
            {
                Insert(Entity);
            }
        }
        public void UpdateList(List<InstanceContent> Entities)
        {
            foreach (InstanceContent Entity in Entities)
            {
                Update(Entity.InstanceContentID,Entity);
            }
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