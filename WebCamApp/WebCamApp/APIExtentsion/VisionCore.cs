using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebCamApp.APIExtentsion
{
    public static class VisionCore
    {

        const string API_Key = "dfea38a270ae4871abce25d64fcd52cf";
        const string API_Location = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/";

        public static IEnumerable<VisualFeature> GetVisualFeatures()
        {
            return new VisualFeature[]
                {
                    VisualFeature.Adult,
                    VisualFeature.Categories,
                    VisualFeature.Color,
                    VisualFeature.Description,
                    VisualFeature.Faces,
                    VisualFeature.ImageType,
                    VisualFeature.Tags
                };
        }

        public static AnalysisResult SmartProcessingImageShowResult(string fname, string method)
        {
            AnalysisResult analysisResult = null;
            Task.Run(async () =>
            {

                string imgname = Path.GetFileName(fname);

                analysisResult = await SmartImageProcessing(fname, method);

                switch (method)
                {

                    case "analyze":
                        ShowResults(analysisResult, analysisResult.Categories, "Categories");
                        ShowFaces(analysisResult);
                        break;

                    case "describe":
                        ShowCaptions(analysisResult);
                        break;

                    case "tag":
                        ShowTags(analysisResult, 0.9);
                        break;
                }


            }).Wait();

            return analysisResult;
        }

        public static async Task<AnalysisResult> SmartImageProcessing(string fname, string method)
        {
            AnalysisResult analyzed = null;
            VisionServiceClient vsClient = new VisionServiceClient(API_Key, API_Location);

            IEnumerable<VisualFeature> visualFeature = GetVisualFeatures();

            if (System.IO.File.Exists(fname))
            {
                using (Stream stream = System.IO.File.OpenRead(fname))
                {
                    switch (method)
                    {
                        case "analyze":
                            analyzed = await vsClient.AnalyzeImageAsync(stream, visualFeature);
                            break;

                        case "describe":
                            analyzed = await vsClient.DescribeAsync(stream);
                            break;

                        case "tag":
                            analyzed = await vsClient.GetTagsAsync(stream);
                            break;

                    }

                }
            }
            return analyzed;
        }


        public static void ShowCaptions(AnalysisResult analysisResult)
        {
            var captions = from caption in analysisResult.Description.Captions select caption.Text + "-" + caption.Confidence;
        }

        public static void ShowResults(AnalysisResult analyze, NameScorePair[] nps, string ResName)
        {
            var results = from result in nps select result.Name + "-" + result.Score.ToString();
        }

        public static void ShowFaces(AnalysisResult analysisResult)
        {
            var faces = from face in analysisResult.Faces select face.Gender + "-" + face.Age.ToString();
        }

        public static void ShowTags(AnalysisResult analyze, double confidence)
        {
            var tags = from tag in analyze.Tags where tag.Confidence > confidence select tag.Name;
        }


        //Byte Array
        //case "generateThumbnail":
        //    analyzed = await vsClient.GetThumbnailAsync(stream, 80, 80, true);
        //    break;

        //ModeResult
        //case "models":
        //    analyzed = await vsClient.ListModelsAsync();
        //    break;

        /// -----------------------------------------------------------------/// -----------------------------------------------------------------------------------
        //ocr Result Optical Character Recognition
        //Detects Texts  contect in an image, Machine readable character stream, pixcels convert into characters,
        //Auto detection of language, supports 25 languages, 
        //Requiremement -
        // input image >= 40*40 and <= 3200 * 3200 pixels and 
        //not largr then 10megapixels
        //JPG, PNG, GIF, BMP,
        // Not larger then 4mb
        //LIMITATIONs -
        //Blurry images, Handwritter or cursive, artistic fonts, small text, complex bg, shadows, oversize, missing capital letter at begining , and strikethrough
        //case "generateThumbnail":
        //    analyzed = await vsClient.RecognizeTextAsync(stream, 80, 80, true);
        //    break;
        public static string[] TextExtraction(string fname, bool wrds, string method)
        {
            string[] res = null;
            Task.Run(async () =>
            {
                res = await TextExtractionCore(fname, wrds, method);
            }).Wait();
            return res;
        }

        private static async Task<string[]> TextExtractionCore(string fname, bool wrds, string method)
        {
            VisionServiceClient vsClient = new VisionServiceClient(API_Key, API_Location);
            string[] textres = null;

            if (System.IO.File.Exists(fname))
            {
                using (Stream stream = System.IO.File.OpenRead(fname))
                {
                    switch (method)
                    {
                        case "TextPic":
                            OcrResults ocr = await vsClient.RecognizeTextAsync(stream, "unk", false);
                            textres = GetExtracted(ocr, wrds);
                            break;

                        case "HandwrittenPic":
                            HandwritingRecognitionOperation hdro = await vsClient.CreateHandwritingRecognitionOperationAsync(stream);
                            HandwritingRecognitionOperationResult hresult = await vsClient.GetHandwritingRecognitionOperationResultAsync(hdro);
                            textres = GetHandWrittenExtracted(hresult, wrds);
                            break;
                    }

                }
            }
            return textres;
        }

        private static string[] GetHandWrittenExtracted(HandwritingRecognitionOperationResult hdro, bool wrds)
        {
            List<string> lst = new List<string>();


            foreach (HandwritingTextLine lineItem in hdro.RecognitionResult.Lines)
            {
                if (wrds)
                {
                    lst.AddRange(GetHandWrittingWords(lineItem));
                }
                else
                {
                    lst.Add(GetHandWrittingLineString(lineItem));
                }
            }

            return lst.ToArray();
        }

        private static string GetHandWrittingLineString(HandwritingTextLine lineItem)
        {
            List<string> words = GetHandWrittingWords(lineItem);
            return words.Count > 0 ? string.Join(" ", words) : string.Empty;
        }

        private static List<string> GetHandWrittingWords(HandwritingTextLine lineItem)
        {
            List<string> words = new List<string>();

            foreach (HandwritingTextWord item in lineItem.Words)
            {
                words.Add(item.Text);
            }

            return words;
        }

        private static string[] GetExtracted(OcrResults ocr, bool wrds)
        {
            List<string> lst = new List<string>();

            foreach (Region regionItems in ocr.Regions)
            {
                foreach (Line lineItem in regionItems.Lines)
                {
                    if (wrds)
                    {
                        lst.AddRange(GetWords(lineItem));
                    }
                    else
                    {
                        lst.Add(GetLineString(lineItem));
                    }
                }
            }
            return lst.ToArray();
        }

        private static string GetLineString(Line lineItem)
        {
            List<string> words = GetWords(lineItem);
            return words.Count > 0 ? string.Join(" ", words) : string.Empty;
        }

        private static List<string> GetWords(Line lineItems)
        {
            List<string> words = new List<string>();

            foreach (Word item in lineItems.Words)
            {
                words.Add(item.Text);
            }

            return words;
        }

        private static void PrintResults(string[] res)
        {
            foreach (var item in res)
            {

            }
        }

        //--------------------------------------------------------------//--------------------------------------------------
        /// <summary>
        ///  //Identify closest value of same line, because machine doesnt understand value infront of any particular entry 
        ///  like Teddy bear      56.00, 
        ///  this it will understand as two region.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public static string[] TextClosestExtraction(string fname, bool wrds, bool mlines)
        {
            string[] res = null;
            Task.Run(async () =>
            {
                res = await TextClosestExtractionCore(fname, wrds, mlines);
                if (mlines && !wrds)
                    res = MergeLines(res);

                //var dates = GetDate(res);
                //var amount = HighestAmount(res);
            }).Wait();

            return res;
        }

        public static string[] MergeLines(string[] res)
        {
            SortedDictionary<int, string> dict = MergeLinesCore(res);
            return dict.Values.ToArray();
        }

        public static SortedDictionary<int, string> MergeLinesCore(string[] res)
        {
            SortedDictionary<int, string> dict = new SortedDictionary<int, string>();

            foreach (var item in res)
            {
                string[] parts = item.Split('|');

                if (parts.Length == 3)
                {
                    int top = Convert.ToInt32(parts[0]);
                    string str = parts[1];
                    int region = Convert.ToInt32(parts[2]);

                    if (dict.Count > 0 && region != 10)
                    {
                        KeyValuePair<int, string> keyValuePairs = FindClosest(dict, top);
                        if (keyValuePairs.Key != -1)
                        {
                            dict[keyValuePairs.Key] = keyValuePairs.Value + " " + str;
                        }
                        else
                        {
                            dict.Add(top, str);
                        }
                    }
                    else
                    {
                        dict.Add(top, str);
                    }
                }
            }
            return dict;
        }

        public static KeyValuePair<int, string> FindClosest(SortedDictionary<int, string> dict, int top)
        {
            KeyValuePair<int, string> keyValuePair = new KeyValuePair<int, string>(-1, string.Empty);

            foreach (KeyValuePair<int, string> item in dict)
            {
                int diff = item.Key - top;
                if (Math.Abs(diff) < 15)
                {
                    keyValuePair = item;
                    break;
                }
            }
            return keyValuePair;
        }

        public static async Task<string[]> TextClosestExtractionCore(string fname, bool wrds, bool mlines)
        {
            VisionServiceClient vsClient = new VisionServiceClient(API_Key, API_Location);
            string[] textres = null;

            if (System.IO.File.Exists(fname))
            {
                using (Stream stream = System.IO.File.OpenRead(fname))
                {

                    OcrResults ocr = await vsClient.RecognizeTextAsync(stream, "unk", false);
                    textres = GetClosestExtracted(ocr, wrds, mlines);
                }
            }
            return textres;
        }

        public static string[] GetClosestExtracted(OcrResults ocr, bool wrds, bool mlines)
        {
            List<string> lst = new List<string>();
            int reg = 1;
            foreach (Region regionItems in ocr.Regions)
            {
                foreach (Line lineItem in regionItems.Lines)
                {
                    if (wrds)
                    {
                        lst.AddRange(GetWords(lineItem));
                    }
                    else
                    {
                        lst.Add(GetLineAsString(lineItem, mlines, reg));
                    }
                    reg++;
                }
            }
            return lst.ToArray();
        }

        public static string GetLineAsString(Line lineItem, bool mlines, int reg)
        {
            List<string> words = GetWords(lineItem);
            string text = string.Join("", words);

            if (mlines)
                text = lineItem.Rectangle.Top.ToString() + "|" + text + "|" + reg.ToString();

            return words.Count > 0 ? text : string.Empty;

        }

        //public static string ParseDate(string str)
        //{
        //    string result = string.Empty;
        //    string[] formats = new string[] { "dd MMM yy h:mm", "dd MMM yy hh:mm" };

        //    foreach (var item in formats)
        //    {
        //        try
        //        {
        //            str = str.Replace("'", "");

        //            if (DateTime.TryParseExact(str, item, CultureInfo.InvariantCulture,
        //                DateTimeStyles.None, out DateTime dateTime))
        //            {
        //                result = str;
        //                break;
        //            }
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    }
        //    return result;
        //}

        //public static string GetDate(string[] res)
        //{
        //    string result = string.Empty;

        //    foreach (var item in res)
        //    {
        //        result = ParseDate(item);
        //        if (result != string.Empty) break;

        //    }
        //    return result;
        //}

        //public static string HighestAmount(string[] res)
        //{
        //    string result = string.Empty;
        //    float highest = 0;

        //    Regex r = new Regex(@"[0-9]+\.[0-9]");

        //    foreach (var item in res)
        //    {
        //        Match m = r.Match(item);

        //        if (m != null && m.Value != string.Empty && Convert.ToDouble(m.Value) > highest)
        //        {
        //            result = m.Value;
        //        }
        //    }
        //    return result;
        //}
    }
}