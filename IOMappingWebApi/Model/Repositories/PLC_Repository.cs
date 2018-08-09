using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IPLC_Repository : IRecordRepository<PLC>
    { }

    public class PLC_Repository : RecordRepository<PLC>, IPLC_Repository
    {
        public PLC_Repository(GalaxyObjectContext ctx) : base(ctx) { }

        //public override void PushToDbset(List<PLC> Entities)
        //{
        //    List<PLC> EntsToPush = GetListSyncFromDB(Entities);

        //    List<PLC> Entities_NOTinDb = NOTInDatabase(EntsToPush);
        //    if (Entities_NOTinDb.Count > 0) { InsertList(Entities_NOTinDb); }

        //    List <PLC> Entities_inDb = InDatabase(EntsToPush);
        //    List<PLC> EntsToUpdate = Entities_inDb
        //        .Where(PLC => PLC.Name != null)
        //        .Select(PLC => PLC).ToList();

        //    if (Entities_inDb.Count > 0) { UpdateList(Entities_inDb); }
        //}
    }
}
