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
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] GalaxyObjects request)
        {
            GalaxyObjects GObjcts = request;
            List <InstanceContent> vContents = GObjcts.List;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            response.ReasonPhrase = "Request did not initialize";

            Contents.PushToDbset(vContents);
            Contents.context.SaveChanges();
            //HttpResponseMessage Response = await Contents.PushToDbset(vContents);

            //EXTRA - For future use
            //List<InstanceContent> Surplus = Contents.SurplusInDatabase(PContents);
            //Contents.DeleteList(Surplus);
            //context.SaveChanges();

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
