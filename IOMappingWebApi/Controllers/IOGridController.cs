using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOMappingWebApi.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Http;

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
                    IOTagID = c.IOTagID,
                    AssetName = c.AssetName
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

        //[HttpPost, ValidateInput(false)]
        [HttpPost]
        public String excel(String extension, String excel)
        {
            if (extension != "csv" && extension != "xml")
            {
                throw new Exception("Unsupported extension");
            }
            String filename = "pqGrid." + extension;
            HttpContext.Session.SetString("excel", excel);

            return filename;
        }

        [HttpGet]
        public FileContentResult excel(String filename)
        {
            String contents = HttpContext.Session.GetString("excel");

            if (filename.EndsWith(".csv"))
            {
                return File(new System.Text.UTF8Encoding().GetBytes(contents), "text/csv", filename);
            }
            else if (filename.EndsWith(".xml"))
            {
                return File(new System.Text.UTF8Encoding().GetBytes(contents), "text/xml", filename);
            }
            else
            {
                throw new Exception("unknown extension");
            }
        }
    }
}
