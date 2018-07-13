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
        private IGalaxyObject_UoW UOW;
        public ObjectXMLController(IGalaxyObject_UoW UnitOfWork)
        {
            UOW = UnitOfWork ?? throw new ArgumentNullException(nameof(UOW));
        }

        // GET api/ObjectXML/'ObjectName'
        [HttpGet]
        [Route("{InstanceName}")]
        public ActionResult Get(string InstanceName)
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)UOW.Contents.EntityCollection.Where(c => c.Instance.Name == InstanceName).ToList();

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

        // GET api/Object/
        [HttpGet]
        public ActionResult Get()
        {
            List<InstanceContent> ContentList = (List<InstanceContent>)UOW.Contents.EntityCollection.Take(10000).OrderBy(c => c.Instance.Name).ToList();

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
            UOW.PushRecordsToDbset(vContents);
            return new HttpResponseMessage(HttpStatusCode.OK);
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
