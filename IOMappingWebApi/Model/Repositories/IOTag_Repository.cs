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

            var FoundEntity = context.Set<IOTag>().FirstOrDefault(e => e.Name == _Entity.Name);
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
    }
}