using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IIOTag_Repository : IRecordRepository<IOTag>
    { IPLC_Repository PLCs { get; } }

    public class IOTag_Repository : RecordRepository<IOTag>, IIOTag_Repository
    {
        public IPLC_Repository PLCs { get; private set; }
        public IOTag_Repository(GalaxyObjectContext ctx, IPLC_Repository _PLCs) : base(ctx) { PLCs = _PLCs; }

        public override List<IOTag> EntityCollection
        {
            get
            {
                var IOTags = context.IOTag
                    .Include(pt => pt.PLC).ToList();
                return IOTags;
            }
            set
            {
                // Not Implemente for the time being
            }
        }

        public override IOTag GetSyncFromDB(IOTag _Entity)
        {
            IOTag ret = new IOTag();

            var FoundEntity = context.Set<IOTag>().FirstOrDefault(e => e.Name == _Entity.Name && e.PLC.Name == _Entity.PLC.Name);
            if (FoundEntity == null)
            {
                ret = new IOTag()
                {
                    Name = _Entity.Name,
                    PLC = PLCs.GetSyncFromDB(_Entity.PLC),
                    PLCID = PLCs.GetID(_Entity.PLC.Name)
                };
            }
            else
            {
                FoundEntity.PLC = PLCs.GetSyncFromDB(_Entity.PLC);
                ret = FoundEntity;
            }
            return ret;
        }

        public override List<IOTag> GetListSyncFromDB(List<IOTag> _Entities)
        {
            List<IOTag> UpdatedList = _Entities
                .Select(ent => GetSyncFromDB(ent))
                .ToList();
            return UpdatedList;
        }

        public override void PushToDbset(List<IOTag> Entities)
        {
            List<PLC> PLCs_ToPush = Entities
                    .OfType<IOTag>()
                    .Select(e => e.PLC)
                    .OfType<PLC>()
                    .GroupBy(vPLC => vPLC.Name)
                    .Select(vPLC => vPLC.First()).ToList();

            if (PLCs_ToPush.Any())
            {
                PLCs.PushToDbset(PLCs_ToPush);
                context.SaveChanges();
            }

            List<IOTag> EntsToPush = GetListSyncFromDB(Entities);

            List<IOTag> Entities_NOTinDb = NOTInDatabase(EntsToPush);
            if (Entities_NOTinDb.Count > 0) { InsertList(Entities_NOTinDb); }

            List<IOTag> Entities_inDb = InDatabase(EntsToPush);
            if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
        }
        // #05 - Methods (BULK Operations)
        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are found on the Database</returns>  
        public override List<IOTag> InDatabase(List<IOTag> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<IOTag>() 
                           on
                           new { Name = ents.Name, PLCName = ents.PLC.Name }
                           equals
                           new { Name = db.Name, PLCName = db.PLC.Name }
                           select db).ToList();
            return (List<IOTag>)Results;
        }

        /// <summary>
        /// Evaluates which records from "List() passed as parameter" are NOT found on the database. 
        /// </summary>
        /// <param name="Entities"> Entities List() </param>
        /// <returns>Records from List() Parameter that are NOT found on the Database</returns>  
        public override List<IOTag> NOTInDatabase(List<IOTag> Entities)
        {
            var Results = (from ents in Entities
                           join db in context.Set<IOTag>() 
                           on
                           new { Name = ents.Name, PLCName = ents.PLC.Name }
                           equals
                           new { Name = db.Name, PLCName = db.PLC.Name }
                           into DbResults
                           where !DbResults.Any()
                           select ents).GroupBy(e => e.Name).Select(e => e.First()).ToList();

            return (List<IOTag>)Results;
        }
    }
}