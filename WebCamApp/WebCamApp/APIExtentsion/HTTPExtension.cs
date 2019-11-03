using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WebCamApp.APIExtentsion
{
    public class HTTPExtension
    {
        private const string visionSubscriptionKey = "dfea38a270ae4871abce25d64fcd52cf";
        private const string faceSubscriptionKey = "b581847633a148aab20a8cf30e06fe6a"; //  b581847633a148aab20a8cf30e06fe6a

        public readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("b581847633a148aab20a8cf30e06fe6a", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/");

        public static string HTTPClientCall(string uriBase, string requestParam, byte[] imageBytes, string CognitiveServiceName)
        {
            HttpClient client = new HttpClient();
            string contentString = string.Empty;

            string SubscriptionKey = IdentifySubscriptionKey(CognitiveServiceName);

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

            // Assemble the URI for the FACE REST API Call.
            string uri = uriBase + requestParam;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = imageBytes;

            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                Task.Run(async () =>
                {
                    response = await client.PostAsync(uri, content);

                    // Get the JSON response.
                    contentString = await response.Content.ReadAsStringAsync();

                }).Wait();
                // Display the JSON response.
                return contentString;

            }
        }

        public async Task<VerifyResult> HTTPClientVerifyCallAsync(string verifyFaceAPI, string faceAPI, Guid guid1, Guid guid2)
        {
            return await faceServiceClient.VerifyAsync(guid1, guid2);
        }

        //public static string HTTPClientVerifyCallAsync(string uriBase, string faceAPI, Guid face1, Guid face2)
        //{
        //    //HttpClient client = new HttpClient();
        //    //string contentString = string.Empty;

        //    //string SubscriptionKey = IdentifySubscriptionKey("FaceAPI");

        //    //// Request headers.
        //    //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

        //    //// Assemble the URI for the FACE REST API Call.
        //    //string uri = uriBase;

        //    //HttpResponseMessage response;

        //    //JavaScriptSerializer js = new JavaScriptSerializer();
        //    //var returnValue = js.Serialize(jsonValue);

        //    //var content = new StringContent(js.ToString(), Encoding.UTF8, "application/json");
        //    //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //    //// Execute the REST API call.
        //    //Task.Run(async () =>
        //    //    {
        //    //        response = await client.PostAsync(uri, content);

        //    //        // Get the JSON response.
        //    //        contentString = await response.Content.ReadAsStringAsync();

        //    //    }).Wait();
        //    //    // Display the JSON response.
        //    //    return contentString;
        //}

        public static string IdentifySubscriptionKey(string CognitiveService)
        {
            string subscriptionKey = string.Empty;
            if (CognitiveService == "FaceAPI")
            {
                subscriptionKey = faceSubscriptionKey;
            }
            else
            {
                subscriptionKey = visionSubscriptionKey;
            }

            return subscriptionKey;
        }
    }
}