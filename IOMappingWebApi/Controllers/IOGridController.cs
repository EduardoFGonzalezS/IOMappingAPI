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
        private IContent_Repository Contents;
        private GalaxyObjectContext context;

        public IOGridController(GalaxyObjectContext ctx, IContent_Repository _Contents)
        {
            context = ctx;
            Contents = _Contents ?? throw new ArgumentNullException(nameof(Contents));
            Contents.context = ctx;
        }

        // GET /IOGrid/List
        public ViewResult List()
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection
                .Take(10000)
                .OrderBy(c => c.Instance.Name)
                .Select(c => new InstanceContent{
                    InstanceContentID = c.InstanceContentID,
                    Instance = c.Instance,
                    InstanceID = c.InstanceID,
                    Attribute = c.Attribute,
                    AttributeID = c.AttributeID,
                    PLCTag = c.PLCTag ?? new PLCTag("N/A") { PLC = new PLC("N/A")},
                    PLCTagID = c.PLCTagID ?? 0,
                    IOTag = c.IOTag,
                    IOTagID = c.IOTagID
                }).ToList();

            return View(ContentList);
        }

        // POST /IOGrid/Post/'Serialized List'
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]  List<InstanceContent> request)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            Contents.PushToDbset(request);
            Contents.context.SaveChanges();

            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }
    }
}
