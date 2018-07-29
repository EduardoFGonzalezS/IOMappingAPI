using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOMappingWebApi.Model.Misc;

namespace IOMappingWebApi.Model
{
    public interface IRecordRepository<TEntity> where TEntity : class
    {
        List<TEntity> EntityCollection { get; set; }
        void Insert(TEntity Entity);
        void PushToDbset(List<TEntity> Entities);
        void UpdateList(List<TEntity> Entities);
        int GetID(String Name);
    }

    public abstract class RecordRepository<TEntity> where TEntity : class, ISimpleRecord, new()
    {
        // #01 - Fields
        protected GalaxyObjectContext context;

        // #02 - Constructors
        public RecordRepository(GalaxyObjectContext ctx)
        {
            context = ctx;
        }

        // #03 - Properties
        public virtual List<TEntity> EntityCollection
        {
            get
            {
                return context.Set<TEntity>().ToList();
            }
            set
            {
                // Not Implemente for the time being
            }
        }

        // #04 - Methods (Single Operations)
        public void Insert(TEntity Entity)
        {
            context.Set<TEntity>().Add(Entity);
        }
        public virtual void Update(int id, TEntity Entity)
        {
            using (context)
            {
                var FoundEntity = context.Set<TEntity>().Find(id);
                context.Entry(FoundEntity).CurrentValues.SetValues(Entity);
            }
        }
        public int GetID(String Name)
        {
            var FoundEntity = context.Set<TEntity>().FirstOrDefault(e => e.Name == Name);
            int returnint;

            if (FoundEntity == null) { returnint = -1;  } else { returnint = FoundEntity.ID; }
            return returnint;
        }

        // #05 - Methods (BULK Operations)
        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are found on the Database</returns>  
        public List<TEntity> InDatabase(List<TEntity> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<TEntity>() on ents.Name equals db.Name
                           select db).ToList();
            return (List<TEntity>)Results;
        }

        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are NOT found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are NOT found on the Database</returns>  
        public List<TEntity> NOTInDatabase(List<TEntity> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<TEntity>() on ents.Name equals db.Name
                           into DbResults
                           where !DbResults.Any()
                           select ents).GroupBy(e => e.Name).Select(e => e.First()).ToList();

            return (List<TEntity>)Results;
        }
        public virtual void PushToDbset(List<TEntity> Entities)
        {
            List<TEntity> Entities_NOTinDb = NOTInDatabase(Entities);
            if (Entities_NOTinDb.Count > 0) { InsertList(Entities_NOTinDb); }

            List<TEntity> Entities_inDb = InDatabase(Entities);
            if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
        }
        public void InsertList(List<TEntity> Entities)
        {
            foreach (TEntity Entity in Entities)
            {
                Insert(Entity);
            }
        }

        public void UpdateList(List<TEntity> Entities)
        {
            foreach (TEntity Entity in Entities)
            {
                //Update(Entity.ID, Entity);
            }
        }
    }
}
