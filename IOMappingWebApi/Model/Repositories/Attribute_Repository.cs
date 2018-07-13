using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IAttribute_Repository : IRecordRepository<IOMappingWebApi.Model.Attribute>
    {    }

    public class Attribute_Repository : RecordRepository<IOMappingWebApi.Model.Attribute>, IAttribute_Repository
    {
        public Attribute_Repository(GalaxyObjectContext ctx) : base(ctx) {  }
    }
}
