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
        [Route("{InstanceName}")]
        public ActionResult Get(string InstanceName)
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection.Where(c => c.Instance.Name == InstanceName).ToList();

            GalaxyObjects GObjcts = new GalaxyObjects();
            GObjcts.List = ContentList;

            return new ContentResult
            {
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(GObjcts),
                ContentType = "application/json"
            };
        }

        // GET api/Object/
        [HttpGet]
        public ActionResult Get()
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection.Take(10000).OrderBy(c => c.Instance.Name).ToList();

            GalaxyObjects GObjcts = new GalaxyObjects();
            GObjcts.List = ContentList;
            
            return new ContentResult
            {
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(GObjcts),
                ContentType = "application/json"
            };
        }

        // POST api/Object/'Serialized Object'
        // Modified to be used only by DI Object deployment in System Platform 2017
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] GalaxyObjects request)
        {
            GalaxyObjects GObjcts = request;
            List <InstanceContent> vContents = GObjcts.List;

            string DIObjectName = vContents.Select(c => c.IOTag.PLC.Name).FirstOrDefault();

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            //Contents.PushToDbset(vContents); Maybe To be Used Later!!!!

            //Extract Contained Classes, and push them to the database
            List<Model.Attribute> Attributes_ToPush = vContents.Select(c => c.Attribute).ToList();
            List<Model.Instance> Instances_ToPush = vContents.Select(c => c.Instance).ToList();
            List<Model.IOTag> IOTags_ToPush = vContents.Select(c => c.IOTag).ToList();

            Contents.Attributes.PushToDbset(Attributes_ToPush);
            Contents.Instances.PushToDbset(Instances_ToPush);
            Contents.IOTags.PushToDbset(IOTags_ToPush);
            Contents.context.SaveChanges();

            //Update the contained classes in the entities, so that they are not null or incomplete
            //List<InstanceContent> EntsToPush = Contents.GetListSyncFromDB(vContents);

            List<InstanceContent> EntsToPush = (from Cont in vContents
                                                select new InstanceContent()
                                                 {
                                                     InstanceContentID = Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name),
                                                     Instance = Contents.Instances.GetSyncFromDB(Cont.Instance),
                                                     InstanceID = Contents.Instances.GetID(Cont.Instance.Name),
                                                     Attribute = Contents.Attributes.GetSyncFromDB(Cont.Attribute),
                                                     AttributeID = Contents.Attributes.GetID(Cont.Attribute.Name),
                                                     PLCTag = Contents.EntityCollection
                                                                        .Where(EC => EC.InstanceContentID == Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name))
                                                                        .Select(EC => EC.PLCTag).FirstOrDefault(),
                                                     PLCTagID = Contents.EntityCollection
                                                                        .Where(EC => EC.InstanceContentID == Contents.GetID(Cont.Instance.Name, Cont.Attribute.Name))
                                                                        .Select(EC => EC.PLCTagID).FirstOrDefault(),
                                                     IOTag = Contents.IOTags.GetSyncFromDB(Cont.IOTag),
                                                     IOTagID = Contents.IOTags.GetID(Cont.IOTag.Name)
                                                 }).ToList();

            //Find contents that as NOT in the database, and insert them
            List<InstanceContent> Entities_NOTinDb = Contents.NOTInDatabase(EntsToPush);
            if (Entities_NOTinDb.Count  > 0) { Contents.InsertList(Entities_NOTinDb); }

            //Find contents that are in the database, and update them
            List<InstanceContent> Entities_inDb = Contents.InDatabase(EntsToPush);
            if (Entities_inDb.Count > 0) { Contents.UpdateList(Entities_inDb); }

            //----------------------------------------------------------------------------

            Contents.context.SaveChanges();
            //HttpResponseMessage Response = await Contents.PushToDbset(vContents);

            //EXTRA - For future use
            List<InstanceContent> Surplus = Contents.SurplusInDatabase(EntsToPush).Where(c => c.IOTag.PLC.Name == DIObjectName).ToList();

            if (Surplus.Count > 0)
            { 
                Contents.DeleteList(Surplus);
                context.SaveChanges();
            }

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
