using System.Net.Http;

namespace IOMappingWebApi.Model
{
    public class StatisMethods
    {
        public static GalaxyObjects GetGObj_AsJsonAsync(HttpResponseMessage response)
        {
            string jsonContent = response.Content.ReadAsStringAsync().Result;

            //Response
            return Newtonsoft.Json.JsonConvert.DeserializeObject<GalaxyObjects>(jsonContent);
        }
    }
}
