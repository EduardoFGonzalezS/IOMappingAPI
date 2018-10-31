using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IPLCTag_Repository : IRecordRepository<PLCTag>
    {
        IPLC_Repository PLCs { get; }
    }

    public class PLCTag_Repository : RecordRepository<PLCTag>, IPLCTag_Repository
    {
        public IPLC_Repository PLCs { get; private set; }
        public PLCTag_Repository(GalaxyObjectContext ctx, IPLC_Repository _PLCs) : base(ctx)
        {
            PLCs = _PLCs;
        }

        public override List<PLCTag> EntityCollection
        {
            get
            {
                var PLCTags = context.PLCTag
                    .Include(pt => pt.PLC).ToList();
                return PLCTags;
            }
            set
            {
                // Not Implemente for the time being
            }
        }

        public override PLCTag GetSyncFromDB(PLCTag _Entity)
        {
            PLCTag ret = new PLCTag();

            var FoundEntity = context.Set<PLCTag>().FirstOrDefault(e => e.Name == _Entity.Name && e.PLC.Name == _Entity.PLC.Name);
            if (FoundEntity == null)
            {
                ret = new PLCTag() {
                    Name = _Entity.Name,
                    PLC = PLCs.GetSyncFromDB(_Entity.PLC),
                    PLCID = PLCs.GetID(_Entity.PLC.Name),
                    Rack = _Entity.Rack, Slot = _Entity.Slot, Point = _Entity.Point
                };
            }
            else
            {
                FoundEntity.PLC = PLCs.GetSyncFromDB(_Entity.PLC);
                FoundEntity.Rack = _Entity.Rack; FoundEntity.Slot = _Entity.Slot; FoundEntity.Point = _Entity.Point;
                ret = FoundEntity;
            }
            return ret;
        }

        public override List<PLCTag> GetListSyncFromDB(List<PLCTag> _Entities)
        {
            List<PLCTag> UpdatedList = _Entities
                .Select(ent => GetSyncFromDB(ent))
                .ToList();
            return UpdatedList;
        }

        public override void InsertList(List<PLCTag> Entities)
        {
            foreach (PLCTag Entity in Entities)
            {
                if (Entity != null)
                {
                    context.Set<PLCTag>().Add(Entity);
                }
            }
        }

        public override void PushToDbset(List<PLCTag> Entities)
        {

            List<PLC> PLCs_ToPush = Entities
                    .OfType<PLCTag>()
                    .Select(e => e.PLC)
                    .OfType<PLC>()
                    .GroupBy(vPLC => vPLC.Name)
                    .Select(vPLC => vPLC.First()).ToList();

            if (PLCs_ToPush.Any())
            { 
                PLCs.PushToDbset(PLCs_ToPush);
                context.SaveChanges();

                List<PLCTag> EntsToPush = GetListSyncFromDB(Entities);

                List<PLCTag> Entities_NOTinDb = NOTInDatabase(EntsToPush);
                if (Entities_NOTinDb.Count > 0) { InsertList(Entities_NOTinDb); }

                List<PLCTag> Entities_inDb = InDatabase(EntsToPush);
                if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
            }
        }
        
        
        // #05 - Methods (BULK Operations)
        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are found on the Database</returns>  
        public override List<PLCTag> InDatabase(List<PLCTag> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<PLCTag>()
                           on
                           new { Name = ents.Name, PLCName = ents.PLC.Name }
                           equals
                           new { Name = db.Name, PLCName = db.PLC.Name }
                           select db).ToList();
            return (List<PLCTag>)Results;
        }

        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are NOT found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are NOT found on the Database</returns>  
        public override List<PLCTag> NOTInDatabase(List<PLCTag> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<PLCTag>()
                           on
                           new { Name = ents.Name, PLCName = ents.PLC.Name }
                           equals
                           new { Name = db.Name, PLCName = db.PLC.Name }
                           into DbResults
                           where !DbResults.Any()
                           select ents).GroupBy(e => e.Name).Select(e => e.First()).ToList();

            return (List<PLCTag>)Results;
        }
    }
}