using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOMappingWebApi.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace IOMappingWebApi.Controllers
{
    public class IOGridController : Controller
    {
        private IGalaxyObject_UoW UOW;
        public IOGridController(IGalaxyObject_UoW UnitOfWork)
        {
            UOW = UnitOfWork ?? throw new ArgumentNullException(nameof(UOW));
        }

        // GET /IOGrid/List
        public ViewResult List()
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)UOW.Contents.EntityCollection.Take(10000).OrderBy(c => c.Instance.Name).ToList();
            return View(ContentList);
        }

        // POST /IOGrid/Post/'Serialized List'
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]  List<InstanceContent> request)
        {
            //int a = 0;
            HttpResponseMessage Response = await UOW.PushRecordsToDbset(request);
            return Response;
        }
    }
}
