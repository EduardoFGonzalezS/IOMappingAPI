using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOMappingWebApi.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using System.Xml;

namespace IOMappingWebApi.Controllers
{
    [Route("api/[controller]")]
    public class ObjectXMLController : Controller
    {
        private IContent_Repository Contents;
        private GalaxyObjectContext context;

        public ObjectXMLController(GalaxyObjectContext ctx, IContent_Repository _Contents)
        {
            context = ctx;
            Contents = _Contents ?? throw new ArgumentNullException(nameof(Contents));
            Contents.context = ctx;
        }

        // GET api/ObjectXML/'ObjectName'
        //[HttpGet]
        //[Route("{InstanceName}")]
        //public ActionResult Get(string InstanceName)
        //{
        //    List<InstanceContent> ContentList = (List<InstanceContent>)Contents.EntityCollection.Where(c => c.Instance.Name == InstanceName).ToList();

        //    GalaxyObjects GObjcts = new GalaxyObjects();
        //    GObjcts.List = ContentList;

        //    XmlSerializer x = new XmlSerializer(GObjcts.List.GetType());

        //    XmlDocument doc = new XmlDocument();
        //    System.IO.StringWriter sww = new System.IO.StringWriter();
        //    XmlWriter writer = XmlWriter.Create(sww);

        //    x.Serialize(writer, GObjcts.List);

        //    return new ContentResult
        //    {
        //        Content = sww.ToString(),
        //        ContentType = "application/xml"
        //    };
        //}

        // GET api/Object/
        [HttpGet]
        public ActionResult Get([FromHeader] string InstanceCollection)
        {
            System.Collections.ArrayList InsCol = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.ArrayList>(InstanceCollection);

            List<InstanceContent> ContentListFromDB = (List<InstanceContent>)Contents.EntityCollection.Take(100000).OrderBy(c => c.Instance.Name).ToList();

            List<string> Filter = InsCol.Cast<string>().ToList();

            List<InstanceContent> ContentList = (from CL in ContentListFromDB.AsEnumerable()
                                                 where Filter.Any(xx => xx.Contains(CL.Instance.Name))
                                                 select CL).ToList();

            GalaxyObjects GObjcts = new GalaxyObjects();
            GObjcts.List = ContentList;

            XmlSerializer x = new XmlSerializer(GObjcts.List.GetType());

            XmlDocument doc = new XmlDocument();
            System.IO.StringWriter sww = new System.IO.StringWriter();
            XmlWriter writer = XmlWriter.Create(sww);

            x.Serialize(writer, GObjcts.List);

            return new ContentResult
            {
                Content = sww.ToString(),
                ContentType = "application/xml"
            };
        }

        // POST api/Post/'Serialized Object'
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] GalaxyObjects request)
        {
            GalaxyObjects GObjcts = request;
            List<InstanceContent> vContents = GObjcts.List;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            Contents.PushToDbset(vContents);
            Contents.context.SaveChanges();

            response = new HttpResponseMessage(HttpStatusCode.OK);
            return response;
        }

        // PUT api/values/5
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
