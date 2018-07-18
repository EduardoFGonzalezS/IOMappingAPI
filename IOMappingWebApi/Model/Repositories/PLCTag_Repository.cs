using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IPLCTag_Repository : IRecordRepository<PLCTag>
    { }

    public class PLCTag_Repository : RecordRepository<PLCTag>, IPLCTag_Repository
    {
        public PLCTag_Repository(GalaxyObjectContext ctx) : base(ctx) { }

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
    }

}