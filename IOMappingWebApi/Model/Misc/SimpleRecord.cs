using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model.Misc
{
    public interface ISimpleRecord
    {
        int ID { get; set; }
        string Name { get; set; }
    }
}
