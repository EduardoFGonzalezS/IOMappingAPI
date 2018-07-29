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

        public override void Update(int id, PLCTag Entity)
        {
            using (context)
            {
                var FoundEntity = context.Set<PLCTag>().Find(id);
                //context.Entry(FoundEntity).CurrentValues.SetValues(Entity);
                context.Entry(FoundEntity).Property("Name").CurrentValue = Entity.Name;
                context.Entry(FoundEntity).Property("Rack").CurrentValue = Entity.Rack;
                context.Entry(FoundEntity).Property("Slot").CurrentValue = Entity.Slot;
                context.Entry(FoundEntity).Property("Point").CurrentValue = Entity.Point;
            }
        }
    }

}