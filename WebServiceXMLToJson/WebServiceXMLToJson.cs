using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebServiceXMLToJson
{
    public static class WebServiceXMLToJson
    {
        [FunctionName("WebServiceXMLToJson")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            dynamic data;
            string Container;
            string FileLocation;

            try
            {
                log.Info("C# HTTP trigger function processed a request.");

                // Get request body
              data = await req.Content.ReadAsAsync<object>();
             
                log.Info(data.ToString());
                string ServiceUrl = data.ServiceUrl;
                string ServiceUserName = data.ServiceUserName;
                string ServicePassword = data.ServicePassword;
                string StorageConnectionString = data.StorageConnectionString;
                 Container = data.Container;
          FileLocation = data.FileLocation;

                /*@"http://pik3599.zonarsystems.net/interface.php?action=showposition&operation=current&format=xml&logvers=3&version=2";
            string ServiceUserName = "fw_upload";
            string ServicePassword = "p1kef!eet";
            string StorageConnectionString= "DefaultEndpointsProtocol=https;AccountName=piketeamzonarabs;AccountKey=W1Sc1Sk0aX/b7LR1f1qN+nYGGZgW+S6AYF+OKo/h8Bg9jAUbje2xJGieRJc8PiHOd7rO8TMj1e7nADzPpKEbnw==;EndpointSuffix=core.windows.net";
            */
                // Get request body


                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceUrl);
                byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(ServiceUserName + ":" + ServicePassword);
                string credentials = System.Convert.ToBase64String(toEncodeAsBytes);


                byte[] byteArray = Encoding.UTF8.GetBytes(ServiceUrl);
                // Configure the request content type to be xml, HTTP method to be POST, and set the content length
                request.Method = "GET";
                request.UserAgent = "WSDAPI";
                request.ContentType = "application/soap+xml;charset=UTF-8";
                request.Timeout = 600000;
                // Configure the request to use basic authentication, with base64 encoded user name and password, to invoke the service.
                request.Headers.Add("Authorization", "Basic " + credentials);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                //XmlReader x = XmlReader.Create(response.GetResponseStream());
                var MainArray = reader.ReadToEnd();

                XmlDocument Docs = new XmlDocument();

                Docs.LoadXml(MainArray);
                string jsonText = JsonConvert.SerializeXmlNode(Docs);
                dynamic dynJson = JObject.Parse(jsonText);
                
                dynJson.CreateDate = DateTime.UtcNow;
                jsonText = dynJson.ToString();
                var acct = CloudStorageAccount.Parse(StorageConnectionString);
                var client = acct.CreateCloudBlobClient();

                // what you'd call each time you need to update a file stored in a blob
                var blob = client.GetContainerReference(Container).GetBlockBlobReference(FileLocation);
               
                blob.UploadText(jsonText);
            }catch(Exception e)
            {
               return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message);
             
                throw e;
            }
           return  req.CreateResponse(HttpStatusCode.OK);

            // Set name to query string or body data
            /*name = name ?? data?.name;

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);*/
        }
    }
}
