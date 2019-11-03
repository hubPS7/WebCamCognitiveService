using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WebCamApp.APIExtentsion
{
    public class JsonObjectValue
    {
        public string face1 { get; set; }
        public string face2 { get; set; }
    }
    public class APIExtensionCore
    { 
        const string uriFaceBase = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/detect"; 
        const string UrlVisionAPI = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/";
        const string VerifyFaceAPI = "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/verify";
        const string FaceAPI = "FaceAPI";
        const string VisionAPI = "VisionAPI";

        public async void VerifyFace(string val1, string val2)
        {
            HTTPExtension ht = new HTTPExtension();
            // var jsonValue = new JsonObjectValue { face1 = val1, face2 = val2 };
            var contentString = await ht.HTTPClientVerifyCallAsync(VerifyFaceAPI, FaceAPI, Guid.Parse(val1), Guid.Parse(val2));
        }
        /// <summary>
        ///  Gets the analysis of the specified image bytes by using the Face API.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeFaceAnalysisRequest(byte[] imageBytes)
        {
            string requestParameters = "?returnFaceId=true&returnFaceLandmarks=false"+
            "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses"+
                ",emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            string contentString = HTTPExtension.HTTPClientCall(uriFaceBase, requestParameters, imageBytes, FaceAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }

        /// <summary>
        /// Analyzing image using Vision API
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeVisionAnalysisRequest(byte[] imageBytes)
        {
            string requestParameters = "analyze?visualFeatures=Categories,Description,Color,Tags,Faces,ImageType,Adult&language=en&details=Celebraties,Landmarks";
            string contentString = HTTPExtension.HTTPClientCall(UrlVisionAPI, requestParameters, imageBytes, VisionAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }

        /// <summary>
        /// Generate Thumbnail
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static async Task<byte[]> MakeVisionThumbnailAnalysisRequest(byte[] imageBytes, int width, int height)
        {
            const string UrlVisionAPI = "https://centralindia.api.cognitive.microsoft.com/vision/v1.0/generateThumbnail";            
            const string visionSubscriptionAPIKey = "2d85ba54537441a1b032a80068ac7745";

            HttpClient visionClient = new HttpClient();

            //Request Headers
            visionClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", visionSubscriptionAPIKey);

            // Request parameters for VISION. A third optional parameter is "details".
            string visionRequestParameters = $"width={width.ToString()}&height={height.ToString()}&smartCropping=true";

            // Assemble the URI for the VISION REST API Call.
            string visionUri = UrlVisionAPI + visionRequestParameters;

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] byteData = imageBytes;

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json" and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the VISION REST API call.
                response = await visionClient.PostAsync(visionUri, content);

                // Get the JSON response.
                var visionContentString = await response.Content.ReadAsByteArrayAsync();

                // Display the JSON response.
                return visionContentString;

            }


        }

        /// <summary>
        /// Description of Image
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeVisionDescriptionRequest(byte[] imageBytes)
        {
            string requestParameters = "describe?maxcandidates";
            string contentString = HTTPExtension.HTTPClientCall(UrlVisionAPI, requestParameters, imageBytes, VisionAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }

        /// <summary>
        /// Tag details
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeVisionTagRequest(byte[] imageBytes)
        {
            string requestParameters = "tag";
            string contentString = HTTPExtension.HTTPClientCall(UrlVisionAPI, requestParameters, imageBytes, VisionAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }

        /// <summary>
        /// Text Recogntion details
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeVisionTextRequest(byte[] imageBytes)
        {
            string requestParameters = "ocr?language=unk&detectOrientation=true";
            //string requestParameters = "textOperations?operationId";
            //string requestParameters = "recognizeText?handwriting=true/false";
            string contentString = HTTPExtension.HTTPClientCall(UrlVisionAPI, requestParameters, imageBytes, VisionAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }


        /// <summary>
        /// Tag details
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public static string MakeVisionDomainSpecificRequest(byte[] imageBytes)
        {
            string requestParameters = "models";//{model}/analyze .... Celebraties and Landmarks
            string contentString = HTTPExtension.HTTPClientCall(UrlVisionAPI, requestParameters, imageBytes, VisionAPI);

            // Display the JSON response.
            return JsonPrettyPrint(contentString);
        }

        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>s
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        public static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

    }
}