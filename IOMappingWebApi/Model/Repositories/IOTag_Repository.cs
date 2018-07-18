using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IIOTag_Repository : IRecordRepository<IOTag>
    { }

    public class IOTag_Repository : RecordRepository<IOTag>, IIOTag_Repository
    {
        public IOTag_Repository(GalaxyObjectContext ctx) : base(ctx) { }

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
    }
}