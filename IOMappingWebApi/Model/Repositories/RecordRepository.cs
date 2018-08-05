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
        List<TEntity> GetListSyncFromDB(List<TEntity> _Entities);
        TEntity GetSyncFromDB(TEntity _Entity);
        GalaxyObjectContext context { get; set; }
    }

    public abstract class RecordRepository<TEntity> where TEntity : class, ISimpleRecord, new()
    {
        // #01 - Fields
        public GalaxyObjectContext context { get; set; }

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
            using (var ctx = context)
            {
                var FoundEntity = ctx.Set<TEntity>().Find(id);

                if (FoundEntity != null)
                { ctx.Entry(FoundEntity).CurrentValues.SetValues(Entity); }

                ctx.SaveChanges();
            }
        }
        public int GetID(String Name)
        {
            var FoundEntity = context.Set<TEntity>().FirstOrDefault(e => e.Name == Name);
            int returnint;

            if (FoundEntity == null) { returnint = -1;  } else { returnint = FoundEntity.ID; }
            return returnint;
        }

        public virtual TEntity GetSyncFromDB(TEntity _Entity)
        {
            var FoundEntity = context.Set<TEntity>().FirstOrDefault(e => e.Name == _Entity.Name);
            if (FoundEntity == null) { return _Entity; } else { return FoundEntity; }
        }
        public virtual List<TEntity> GetListSyncFromDB(List<TEntity> _Entities)
        {
            List<TEntity> UpdatedList = _Entities.Select(ent => GetSyncFromDB(ent)).ToList();
            return UpdatedList;
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
            List<TEntity> EntsToPush = GetListSyncFromDB(Entities);

            List<TEntity> Entities_NOTinDb = NOTInDatabase(EntsToPush);
            if (Entities_NOTinDb.Count > 0) { InsertList(Entities_NOTinDb); }

            List<TEntity> Entities_inDb = InDatabase(EntsToPush);
            if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
        }
        public virtual void InsertList(List<TEntity> Entities)
        {
            foreach (TEntity Entity in Entities)
            {
                if (Entity != null)
                { context.Set<TEntity>().Add(Entity); }
            }
        }

        public void UpdateList(List<TEntity> Entities)
        {
            foreach (TEntity Entity in Entities)
            {
                int id = Entity.ID;
                var FoundEntity = context.Set<TEntity>().Find(id);

                if (FoundEntity != null && FoundEntity != Entity)
                { context.Entry(FoundEntity).CurrentValues.SetValues(Entity); }
            }
        }
    }
}
