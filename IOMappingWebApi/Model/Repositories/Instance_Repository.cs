using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IInstance_Repository : IRecordRepository<Instance>
    { }

    public class Instance_Repository : RecordRepository<Instance>, IInstance_Repository
    {
        public Instance_Repository(GalaxyObjectContext ctx) : base(ctx) { }
    }
}

