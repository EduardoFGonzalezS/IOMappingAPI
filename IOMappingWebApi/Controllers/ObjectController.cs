using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOMappingWebApi.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace IOMappingWebApi.Controllers
{
    [Route("api/[controller]")]
    public class ObjectController : Controller
    {
        private IContent_Repository Contents;
        private GalaxyObjectContext context;

        public ObjectController(GalaxyObjectContext ctx, IContent_Repository _Contents)
        {
            context = ctx;
            Contents = _Contents ?? throw new ArgumentNullException(nameof(Contents));
            Contents.context = ctx;
        }

        // GET api/Object/'ObjectName'
        [HttpGet]
        public ActionResult Get([FromHeader] string InstanceCollection)
        {

            System.Collections.ArrayList InsCol = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.ArrayList>(InstanceCollection);

            List<InstanceContent> ContentListFromDB= (List<InstanceContent>)Contents.EntityCollection.Take(100000).OrderBy(c => c.Instance.Name).ToList();

            List<string> Filter = InsCol.Cast<string>().ToList();

            List<InstanceContent> ContentList = (from CL in ContentListFromDB.AsEnumerable()
                      where Filter.Any(x => x.Contains(CL.Instance.Name))
                      select CL).ToList();

            GalaxyObjects GObjcts = new GalaxyObjects();
            GObjcts.List = ContentList;

            return new ContentResult
            {
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(GObjcts),
                ContentType = "application/json"
            };
        }

        //[HttpGet]
        //[Route("{InstanceName}")]
        //public ActionResult Get(string InstanceName)
        //{
        //    List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection.Where(c => c.Instance.Name == InstanceName).ToList();

        //    GalaxyObjects GObjcts = new GalaxyObjects();
        //    GObjcts.List = ContentList;

        //    return new ContentResult
        //    {
        //        Content = Newtonsoft.Json.JsonConvert.SerializeObject(GObjcts),
        //        ContentType = "application/json"
        //    };
        //}

        // GET api/Object/


        //[HttpGet]
        //public ActionResult Get()
        //{
        //    List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection.Take(10000).OrderBy(c => c.Instance.Name).ToList();

        //    GalaxyObjects GObjcts = new GalaxyObjects();
        //    GObjcts.List = ContentList;
            
        //    return new ContentResult
        //    {
        //        Content = Newtonsoft.Json.JsonConvert.SerializeObject(GObjcts),
        //        ContentType = "application/json"
        //    };
        //}

        // POST api/Object/'Serialized Object'
        // Modified to be used only by DI Object deployment in System Platform 2017
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] GalaxyObjects request)
        {
            Console.WriteLine("#01 - Initializing Program");
            GalaxyObjects GObjcts = request;
            List <InstanceContent> vContents = GObjcts.List;

            string DIObjectName = vContents.Select(c => c.IOTag.PLC.Name).FirstOrDefault();

            Console.WriteLine("DIOBJECTNAME = " + DIObjectName);
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            //Contents.PushToDbset(vContents); Maybe To be Used Later!!!!

            //Extract Contained Classes, and push them to the database
            Console.WriteLine("#02 - Extract Contained Classes, and push them to the database");
            List <Model.Attribute> Attributes_ToPush = vContents.Select(c => c.Attribute).ToList();
            List<Model.Instance> Instances_ToPush = vContents.Select(c => c.Instance).ToList();
            List<Model.IOTag> IOTags_ToPush = vContents.Select(c => c.IOTag).ToList();

            Contents.Attributes.PushToDbset(Attributes_ToPush);
            Contents.Instances.PushToDbset(Instances_ToPush);
            Contents.IOTags.PushToDbset(IOTags_ToPush);
            Contents.context.SaveChanges();

            //Update the contained classes in the entities, so that they are not null or incomplete
            //List<InstanceContent> EntsToPush = Contents.GetListSyncFromDB(vContents);
            //Contents.context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            Console.WriteLine("#03 - Sync with DB: Update the contained classes in the entities, so that they are not null or incomplete");

            //List<InstanceContent> EntsToPush = new List<InstanceContent>();

            //db Values
            List <Model.Instance> dbInstances = Contents.Instances.EntityCollection;
            List<Model.Attribute> dbAttributes = Contents.Attributes.EntityCollection;
            List<Model.IOTag> dbIOTags = Contents.IOTags.EntityCollection;
            List<Model.PLCTag> dbPLCTags = Contents.PLCTags.EntityCollection;
            List<InstanceContent> dbContents = Contents.EntityCollection;


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
                                                    //into IC InstanceContentID = cs != null ? cs.InstanceContentID : -1,
                                                select new InstanceContent()
                                                {
                                                    InstanceContentID = cs != null ? cs.InstanceContentID : -1,
                                                    Instance = i != null ? i : new Model.Instance(),
                                                    InstanceID = i != null ? i.ID : 0,
                                                    Attribute = a != null ? a : new Model.Attribute(),
                                                    AttributeID = a != null ? a.ID: 0,
                                                    PLCTag = cs != null ? cs.PLCTag: new Model.PLCTag(),
                                                    PLCTagID = cs != null ? cs.PLCTagID : 0,
                                                    IOTag = io != null ? io: new Model.IOTag(),
                                                    IOTagID = io != null ? io.ID: 0,
                                                    AssetName = cs != null ? cs.AssetName : ""
                                                  }).ToList();

            //foreach (InstanceContent Cont in vContents)
            //{

            //    Console.WriteLine("SYNC ENTITY : " + Cont.Instance.Name + "." + Cont.Attribute.Name);
            //    EntsToPush.Add(
            //        new InstanceContent()
            //        {
            //            InstanceContentID = Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name),
            //            Instance = Contents.Instances.GetSyncFromDB(Cont.Instance),
            //            InstanceID = Contents.Instances.GetID(Cont.Instance.Name),
            //            Attribute = Contents.Attributes.GetSyncFromDB(Cont.Attribute),
            //            AttributeID = Contents.Attributes.GetID(Cont.Attribute.Name),
            //            PLCTag = Contents.EntityCollection
            //                          .Where(EC => EC.InstanceContentID == Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name))
            //                          .Select(EC => EC.PLCTag).FirstOrDefault(),
            //            PLCTagID = Contents.EntityCollection
            //                                    .Where(EC => EC.InstanceContentID == Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name))
            //                                    .Select(EC => EC.PLCTagID).FirstOrDefault(),
            //            IOTag = Contents.IOTags.GetSyncFromDB(Cont.IOTag),
            //            IOTagID = Contents.IOTags.GetID(Cont.IOTag.Name),
            //            AssetName = Contents.GetAssetName(Cont.Instance.Name, Cont.Attribute.Name)
            //        });
            //}

            int ii = 0;
            foreach (InstanceContent InsCont in EntsToPush)
            {
                ++ii;
                Console.WriteLine("SYNCED COLLECTION " + ii + " : ID: "+ InsCont.InstanceContentID + " - " + InsCont.Instance.Name + "." + InsCont.Attribute.Name + "  PLC = " + InsCont.IOTag.PLC.Name + "  IOTAG = " + InsCont.IOTag.Name + ",ID="+ InsCont.IOTag.ID);
            }


            //Find contents that as NOT in the database, and insert them
            Console.WriteLine("#04 - Find contents that as NOT in the database, and insert them");
            //List<InstanceContent> Entities_NOTinDb = Contents.NOTInDatabase(EntsToPush);

            List<InstanceContent> Entities_NOTinDb = EntsToPush.Where(c => c.InstanceContentID <= 0).ToList();
            if (Entities_NOTinDb.Count  > 0) { Contents.InsertList(Entities_NOTinDb); }

            ii = 0;
            foreach (InstanceContent InsCont in Entities_NOTinDb)
            {
                ++ii;
                Console.WriteLine("ENTITIES NOT IN DB " + ii + " : ID: " + InsCont.InstanceContentID + " - " + InsCont.Instance.Name + "." + InsCont.Attribute.Name + "  PLC = " + InsCont.IOTag.PLC.Name);
            }

            //Find contents that are in the database, and update them
            Console.WriteLine("#05 - Find contents that are in the database, and update them");
            //List<InstanceContent> Entities_inDb = Contents.InDatabase(EntsToPush);

            List<InstanceContent> Entities_inDb = EntsToPush.Where(c => c.InstanceContentID > 0).ToList();
            if (Entities_inDb.Count > 0) { Contents.UpdateList(Entities_inDb); }

            ii = 0;
            foreach (InstanceContent InsCont in Entities_inDb)
            {
                ++ii;
                Console.WriteLine("ENTITIES IN DB " + ii + " : ID: " + InsCont.InstanceContentID + " - " + InsCont.Instance.Name + "." + InsCont.Attribute.Name + "  PLC = " + InsCont.IOTag.PLC.Name);
            }

            //----------------------------------------------------------------------------
            Console.WriteLine("#06 - Save Changes in DbContext Object");
            Contents.context.SaveChanges();
            //HttpResponseMessage Response = await Contents.PushToDbset(vContents);

            //EXTRA - For future use
            Console.WriteLine("#07 - Resolve Surplus (Recent)");
            //List<InstanceContent> Surplus = Contents.SurplusInDatabase(EntsToPush).Where(c => c.IOTag.PLC.Name == DIObjectName).ToList();

            List<InstanceContent>  Surplus = (from dbconts in Contents.EntityCollection.Where(c => c.IOTag.PLC.Name == DIObjectName)
                                              where !EntsToPush.Any
                                              (ents =>
                                                  dbconts.Attribute.ID == ents.Attribute.ID
                                               && dbconts.Instance.ID == ents.Instance.ID
                                               && dbconts.IOTag.ID == ents.IOTag.ID
                                               )
                                             select dbconts).ToList();

            ii = 0;
            foreach (InstanceContent InsCont in Surplus)
            {
                ++ii;
                Console.WriteLine("SURPLUS ENTITIES " + ii + " : ID: " + InsCont.InstanceContentID + " - " + InsCont.Instance.Name + "." + InsCont.Attribute.Name + "  PLC = " + InsCont.IOTag.PLC.Name + "  IOTAgID = " + InsCont.IOTag.ID + "  IOTAgNAme = " + InsCont.IOTag.Name);
            }

            if (Surplus.Count > 0)
            {
                Contents.DeleteList(Surplus);
                context.SaveChanges();
            }

            Console.WriteLine("#08 - Return Message");
            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
