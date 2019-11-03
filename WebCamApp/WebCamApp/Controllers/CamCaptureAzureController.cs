using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebCamApp.APIExtentsion;

namespace WebCamApp.Controllers
{
    public class CamCaptureAzureController : Controller
    {

        public readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("a96ab08b953c42b690e83ab388f491ce", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0/");

        // GET: CamCaptureAzure
        public ActionResult Capture()
        {
            return View();
        }

        public async Task<VerifyResult> HTTPClientVerifyCallAsync()
        {
            Guid guid1 = Guid.Parse("5c1dae5f-5f51-4a86-9f7b-a759876fbe78");
            Guid guid2 = Guid.Parse("ef8ee8dc-f631-42dc-b561-e1a38e8f6ae3");
            var contentString = await faceServiceClient.VerifyAsync(guid1, guid2);
            return contentString;
        }

        [HttpPost]
        public dynamic Capture(string base64String)
        {
            List<dynamic> strList = new List<dynamic>();
            if (!string.IsNullOrEmpty(base64String))
            {
                var imageParts = base64String.Split(',').ToList<string>();
                byte[] imageBytes = Convert.FromBase64String(imageParts[1]);
                DateTime nm = DateTime.Now;
                string date = nm.ToString("yyyymmddMMss");
                var path = Server.MapPath("~/CapturedPhotos/" + date + "CamCapture.jpg");

                var response = APIExtensionCore.MakeFaceAnalysisRequest(imageBytes);
                strList.Add(response);


                System.IO.File.WriteAllBytes(path, imageBytes);

                //Analyze will bring all features of VISION API.
                var anaylze = VisionCore.SmartProcessingImageShowResult(path, "analyze");
                strList.Add(anaylze);
                //If you want to call individual feature of VISION API below are methods
                var describe = VisionCore.SmartProcessingImageShowResult(path, "describe");
                strList.Add(describe);

                var tag = VisionCore.SmartProcessingImageShowResult(path, "tag");
                strList.Add(tag);


                ////Text Recognition TextPic/HandwrittenPic
                //TextExtraction(path, false, "TextPic");
                //TextExtraction(path, false, "HandwrittenPic");
                ////Identify closest value into text recognition
                ////TextClosestExtraction(path, false, true);
                return Json(data: strList);
            }
            else
            {
                return Json(data: false);
            }
        }

        [HttpPost]
        public dynamic UploadFiles()
        {

            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;

                    //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                    //string filename = Path.GetFileName(Request.Files[i].FileName);  

                    HttpPostedFileBase file = files[0];
                    string fname;

                    // Checking for Internet Explorer  
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }

                    Stream fs = file.InputStream;
                    BinaryReader br = new BinaryReader(fs);
                    byte[] imageBytes = br.ReadBytes((Int32)fs.Length);

                    var strList = FileUploadedPicAI(imageBytes);
                    //fname = Path.Combine(Server.MapPath("~/Uploads/"), fname);
                    //file.SaveAs(fname);

                    // Returns message that successfully uploaded  
                    return strList;
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public dynamic FileUploadedPicAI(byte[] imageBytes)
        {
            List<dynamic> strList = new List<dynamic>();
            DateTime nm = DateTime.Now;
            string date = nm.ToString("yyyymmddMMss");
            var path = Server.MapPath("~/CapturedPhotos/" + date + "CamCapture.jpg");

            var response = APIExtensionCore.MakeFaceAnalysisRequest(imageBytes);
            strList.Add(response);

            //var visionResponse = APIExtensionCore.MakeVisionAnalysisRequest(imageBytes);
            //strList.Add(visionResponse);
            //var byteToConvertThumbnail = await MakeVisionThumbnailAnalysisRequest(imageBytes, 80, 80);


            System.IO.File.WriteAllBytes(path, imageBytes);

            //Analyze will bring all features of VISION API.
            var anaylze = VisionCore.SmartProcessingImageShowResult(path, "analyze");
            strList.Add(anaylze);

            //Text Recognition TextPic/HandwrittenPic
            //var textPic = VisionCore.TextExtraction(path, false, "TextPic");
            //strList.Add(textPic);
            ////Handwritten pics
            //var handwrittenPic = VisionCore.TextExtraction(path, false, "HandwrittenPic");
            //strList.Add(handwrittenPic);
            //Identify closest value into text recognition
            var textClosestExtraction = VisionCore.TextClosestExtraction(path, false, true);
            strList.Add(textClosestExtraction);

            return Json(data: strList);
        }
    }
}