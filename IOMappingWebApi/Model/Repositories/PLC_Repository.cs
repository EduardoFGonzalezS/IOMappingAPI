using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOMappingWebApi.Model
{
    public interface IPLC_Repository 
    { }

    public class PLC_Repository: IPLC_Repository
    {
        // #01 - Fields
        private GalaxyObjectContext context;

        // #02 - Constructors
        public PLC_Repository(GalaxyObjectContext ctx)
        {
            context = ctx;
        }

        // #03 - Properties
        public List<PLC> EntityCollection
        {
            get
            {
                return context.Set<PLC>().ToList();
            }
            set
            {
            }
        }
    }
}
