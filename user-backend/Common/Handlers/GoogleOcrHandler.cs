using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Google.Cloud.Vision.V1;
using Google.Protobuf;

namespace Common.Extensions
{
    public class GoogleOcrHandler : IOcrService
    {
        private readonly ILogger<GoogleOcrHandler> _logger;

        public GoogleOcrHandler(ILogger<GoogleOcrHandler> logger)
        {
            _logger = logger;
        }

        public async Task<string> GenerateOcrHtmlFromBase64ImageAsync(string base64Image)
        {
            try
            {
                var config = new
                {
                    ApplicationCredentials = "comda-ocr-d0f4da883e3d.json",
                    MimeType = "image/tiff",
                    BucketName = "comdaocr-bucket",
                    DocsFolder = "Docs",
                    OcrResultsFolder = "Docs\\json_output\\"
                    //SmallBatchMaxNumOfPages = 5
                };

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", config.ApplicationCredentials);

                var client = await ImageAnnotatorClient.CreateAsync();

                if (base64Image.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                    base64Image = base64Image.Substring(base64Image.IndexOf(",") + 1);

                byte[] imageBytes = Convert.FromBase64String(base64Image);
                byte[] tiffBytes = ConvertToTiff(imageBytes);

                var inputConfig = new InputConfig
                {
                    MimeType = config.MimeType,
                    Content = ByteString.CopyFrom(tiffBytes)
                };

                var request = new AnnotateFileRequest
                {
                    InputConfig = inputConfig
                };
                request.Features.Add(new Feature { Type = Feature.Types.Type.DocumentTextDetection });
                request.Pages.Add(-1);

                var batchRequest = new BatchAnnotateFilesRequest();
                batchRequest.Requests.Add(request);

                var response = await client.BatchAnnotateFilesAsync(batchRequest);
                var json = Google.Protobuf.JsonFormatter.Default.Format(response.Responses[0]);

                return OcrJsonToPositionedHtml(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR processing failed");
                return string.Empty;
            }
        }

        private static byte[] ConvertToTiff(byte[] imageBytes)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = System.Drawing.Image.FromStream(inputStream);
            using var outputStream = new MemoryStream();
            image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Tiff);
            return outputStream.ToArray();
        }

        public static string OcrJsonToPositionedHtml(string ocrJson)
        {
            var jObj = JObject.Parse(ocrJson);
            var response = jObj["responses"]?.FirstOrDefault() ?? jObj;
            var page = response["fullTextAnnotation"]?["pages"]?.FirstOrDefault();
            if (page == null) return "";

            int width = page["width"]?.Value<int>() ?? 1000;
            int height = page["height"]?.Value<int>() ?? 1400;

            var sb = new System.Text.StringBuilder();

            foreach (var block in page["blocks"] ?? new JArray())
            {
                foreach (var paragraph in block["paragraphs"] ?? new JArray())
                {
                    foreach (var word in paragraph["words"] ?? new JArray())
                    {
                        var wordText = string.Concat(word["symbols"]?.Select(s => s["text"]?.ToString()) ?? Enumerable.Empty<string>());
                        var vertices = word["boundingBox"]?["vertices"];
                        if (vertices != null && vertices.Count() >= 2)
                        {
                            int x = vertices[0]["x"]?.Value<int>() ?? 0;
                            int y = vertices[0]["y"]?.Value<int>() ?? 0;
                            int x2 = vertices[2]["x"]?.Value<int>() ?? x + 50;
                            int y2 = vertices[2]["y"]?.Value<int>() ?? y + 20;
                            int w = Math.Abs(x2 - x);
                            int h = Math.Abs(y2 - y);

                            sb.AppendLine($@"
                                            <span style='
                                              position: absolute;
                                              left: {x}px;
                                              top: {y}px;
                                              width: {w}px;
                                              height: {h}px;
                                              font-size: {h}px;
                                              color: transparent;
                                              opacity: 0;
                                            '>{System.Net.WebUtility.HtmlEncode(wordText)}</span>");
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
