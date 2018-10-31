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

            Console.WriteLine("#01 - Initializing Program");
            List<InstanceContent> vContents = request;

            //Extract Contained Classes, and push them to the database
            Console.WriteLine("#02 - Extract Contained Classes, and push them to the database");
            List<Model.PLCTag> PLCTags_ToPush = vContents.Select(c => c.PLCTag).ToList();
            Contents.PLCTags.PushToDbset(PLCTags_ToPush);
            Contents.context.SaveChanges();


            List<Model.Instance> dbInstances = Contents.Instances.EntityCollection;
            List<Model.Attribute> dbAttributes = Contents.Attributes.EntityCollection;
            List<Model.IOTag> dbIOTags = Contents.IOTags.EntityCollection;
            List<Model.PLCTag> dbPLCTags = Contents.PLCTags.EntityCollection;
            List<InstanceContent> dbContents = Contents.EntityCollection;


            //Update the contained classes in the entities, so that they are not null or incomplete
            //List<InstanceContent> EntsToPush = Contents.GetListSyncFromDB(request);

            //Contents.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            Console.WriteLine("#03 - Sync with DB: JOIN TABLES");
            List<InstanceContent> EntsToPush = (from Cont in vContents
                                                join db_Contents in dbContents
                                                on
                                                new { InstName = Cont.Instance.Name, AttName = Cont.Attribute.Name }
                                                equals
                                                new { InstName = db_Contents.Instance.Name, AttName = db_Contents.Attribute.Name }
                                                into dbCs
                                                from cs in dbCs.DefaultIfEmpty(Cont)
                                                join db_Instances in dbInstances on Cont.Instance.Name equals db_Instances.Name
                                                into Ins
                                                from i in Ins.DefaultIfEmpty(Cont.Instance)
                                                join db_Attributes in dbAttributes on Cont.Attribute.Name equals db_Attributes.Name
                                                into Atts
                                                from a in Atts.DefaultIfEmpty(Cont.Attribute)
                                                join db_IOTags in dbIOTags on
                                                new { IOTagName = Cont.IOTag.Name, PLCName = Cont.IOTag.PLC.Name }
                                                equals
                                                new { IOTagName = db_IOTags.Name, PLCName = db_IOTags.PLC.Name }
                                                into IOs
                                                from io in IOs.DefaultIfEmpty(Cont.IOTag)
                                                join db_PLCTags in dbPLCTags on
                                                new { PLCTagName = Cont.PLCTag.Name, PLCName = Cont.PLCTag.PLC.Name }
                                                equals
                                                new { PLCTagName = db_PLCTags.Name, PLCName = db_PLCTags.PLC.Name }
                                                into PLCs
                                                from P in PLCs.DefaultIfEmpty(Cont.PLCTag)
                                                    //into IC InstanceContentID = cs != null ? cs.InstanceContentID : -1,
                                                select new InstanceContent()
                                                {
                                                    InstanceContentID = cs != null ? cs.InstanceContentID : -1,
                                                    Instance = i != null ? i : new Model.Instance(),
                                                    InstanceID = i != null ? i.ID : 0,
                                                    Attribute = a != null ? a : new Model.Attribute(),
                                                    AttributeID = a != null ? a.ID : 0,
                                                    PLCTag = P != null ? P : new Model.PLCTag(),
                                                    PLCTagID = P != null ? P.ID : 0,
                                                    IOTag = io != null ? io : new Model.IOTag(),
                                                    IOTagID = io != null ? io.ID : 0,
                                                    AssetName = Cont.AssetName
                                                }).ToList();

            //Find contents that are in the database, and update them
            //List<InstanceContent> Entities_inDb = Contents.InDatabase(EntsToPush);

            
            List<InstanceContent> Entities_inDb = EntsToPush.Where(c => c.InstanceContentID > 0).ToList();
            if (Entities_inDb.Count > 0) { Contents.UpdateList(Entities_inDb); }

            foreach (InstanceContent IC in Entities_inDb)
            {
                Console.WriteLine("In DB:  ID" + IC.InstanceContentID + ":" + IC.Instance.Name + "." + IC.Attribute.Name + "  PLCTagID:" + IC.PLCTag.ID + ":" + IC.PLCTag.Name);
            }
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
