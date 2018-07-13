using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOMappingWebApi.Model;
using System.Net.Http;
using System.Net;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IOMappingWebApi.Controllers
{
    [Route("api/[controller]")]
    public class AttributeController : Controller
    {
        private IAttribute_Repository repository;

        public AttributeController(IAttribute_Repository repo)
        {
            repository = repo ?? throw new ArgumentNullException(nameof(repository));
        }


        [HttpGet]
        public ActionResult Get()
        {
            var Object = repository.EntityCollection.Take(1000).OrderBy(a => a.Name);

            return new ContentResult
            {
                Content = Newtonsoft.Json.JsonConvert.SerializeObject(Object),
                ContentType = "application/json"
            };
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
