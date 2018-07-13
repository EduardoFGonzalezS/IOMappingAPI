using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace EnvelopeCore
{
    // ## CLASS : StaticMethods ------------
    /// <summary>
    ///This class contains the static helper methods used for these webservices
    /// </summary>
    internal static class StaticMethods
    {
        // *** #01 - METHOD - Private method that uses NewtonSoft to Serialize Object into JSon and send it in Http Post Restful Service
        private static async Task<HttpResponseMessage> PostAsJsonAsync_Exec(HttpClient client, string addr, object obj)
        {
            try
            {
                var response = await client.PostAsync(addr, new StringContent(
                    Newtonsoft.Json.JsonConvert.SerializeObject(obj),
                    Encoding.UTF8, "application/json"));
                return response;
            }
            catch (Exception ex)
            {
                return HttpResponseData(ex.InnerException.Message.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        // *** #02 - METHOD - Private method that Gets information via Get Restful Service
        private static async Task<HttpResponseMessage> GetAsJsonAsync_Exec(HttpClient client, string addr)
        {
            try
            {
                var response = await client.GetAsync(addr,HttpCompletionOption.ResponseContentRead);
                return response;
            }
            catch (Exception ex)
            {
                return HttpResponseData(ex.InnerException.Message.ToString(), HttpStatusCode.InternalServerError);
            }
        }

        // *** #03 - METHOD - Method that handles the HttpResponseData
        public static HttpResponseMessage HttpResponseData(string message, HttpStatusCode stsCode)
        {
            HttpResponseMessage response = new HttpResponseMessage(stsCode);
            response.Content = new StringContent(message);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("applicaiton/json");
            return response;
        }

        // *** #04 - METHOD - Method that will be used by classes that implement IEnvelopeData
        /// <summary>
        ///Method that serializes the IEnvelope as JSon and sends it via Http-Post Request to the specified address (sURI + SFolderPath)
        /// </summary>
        public static HttpResponseMessage PostAsJsonAsync(String sURI, String sFolderPath, IEnvelope Envelope)
        {
            HttpResponseMessage response = new HttpResponseMessage();

            //Http Client Configuration
            HttpClient client = new HttpClient()
                {
                    BaseAddress = new Uri(sURI)
                };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //Http Post Webservice that sends envelope
            response = PostAsJsonAsync_Exec(client, sFolderPath, Envelope).Result;

            //Response
            return response;
        }

        // *** #05 - METHOD - Method that will be used by classes that implement IEnvelopeData
        /// <summary>
        ///Method that fetches the required object-state via Http-Get Request from the specified address (sURI + SFolderPath)
        /// </summary>
        public static IEnvelopePacket GetAsJsonAsync(String sURI, String sFolderPath, String OptHeader)
        {
            //Http Client Configuration
            HttpClient client = new HttpClient()
            {
                BaseAddress = new Uri(sURI)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("LiteID", OptHeader);

            //Http Post Webservice that sends envelope
            HttpResponseMessage response = GetAsJsonAsync_Exec(client, sFolderPath).Result;

            string jsonContent = response.Content.ReadAsStringAsync().Result;

            //Response
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnvelopePacket>(jsonContent);
        }
     }

    //## INTERFACE : IEnvelopeData ------------
    /// <summary>
    ///Any class that implements this interface qualifies as a candidate to be sent via Http Post serialized as Json
    ///It also acts as a class that can Request Information via Http Get
    /// </summary>
    public interface IEnvelope
    {
        HttpResponseMessage PostAsJsonAsync(String sURI, String sFolderPath);
        IEnvelopePacket GetAsJsonAsync(String sURI, String sFolderPath, String OptHeader);
    };

    //## INTERFACE : IEnvelopePacket ------------
    /// <summary>
    ///Returned Packet from Http Get Request
    /// </summary>
    public interface IEnvelopePacket{};
}
