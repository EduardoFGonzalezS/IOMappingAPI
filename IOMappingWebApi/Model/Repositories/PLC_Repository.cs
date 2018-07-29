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
    }
}
