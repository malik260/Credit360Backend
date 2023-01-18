using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.media;
using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http.Headers;
using System.Linq;
using FintrakBanking.Common.CustomException;
using System.Threading;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/media")]
    public class FileUploadController : ApiControllerBase
    {
        IMediaRepository _uploadService;
        public FileUploadController(IMediaRepository uploadService)
        {
            this._uploadService = uploadService;
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("document")]
        public HttpResponseMessage GetDocument(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { result = _uploadService.GetDocumentById(id) });
        }

        //[HttpGet]
        //[Route("document-viewer")]
        //public HttpResponseMessage GetDocumentToViewById(int id)
        //{
        //    return Request.CreateResponse(HttpStatusCode.OK, new { result = _uploadService.GetDocumentToViewById(id) });
        //}

        //[HttpPost, Route("upload-filesystem-async")]
        //public async Task<HttpResponseMessage> PostFile()
        //{
        //    HttpRequestMessage request = this.Request;
        //    if (!request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    //string root = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/uploads");
        //    string root = "C:\\DEV\\LATEST\\upload\\";
        //    var provider = new MultipartFormDataStreamProvider(root);

        //    var task = await request.Content.ReadAsMultipartAsync(provider).
        //        ContinueWith<HttpResponseMessage>(o =>
        //        {

        //            string file1 = provider.FileData.First().LocalFileName;
        //            // this is the file name on the server where the file was saved 

        //            return new HttpResponseMessage()
        //            {
        //                Content = new StringContent("File uploaded.")
        //            };
        //        }
        //    );
        //    return task;
        //}

        [HttpPost, Route("upload-file")]
        public HttpResponseMessage UploadJsonFile()
        {

            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                var httpRequest = HttpContext.Current.Request;

                bool result = false;
                if (httpRequest.Files.Count > 0)
                {
                    result = true;
                    foreach (string file in httpRequest.Files)
                    {
                        var postedFile = httpRequest.Files[file];
                        string filePath = Path.Combine(@"C:\DEV\LATEST\upload\", postedFile.FileName);
                        //var filePath = HttpContext.Current.Server.MapPath("~/UploadFile/" + postedFile.FileName);
                       // postedFile.SaveAs(filePath);
                    }
                }

                // test

                // end

                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document upload was successful" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The was an error while uploading document" });
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("document-excel")]
        public async Task<HttpResponseMessage> ExcelDocumentUpload()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                Task.Factory
                    .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                        CancellationToken.None,
                        TaskCreationOptions.LongRunning, // guarantees separate thread
                        TaskScheduler.Default)
                    .Wait();


                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

              

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsStreamAsync();
                var data = _uploadService.ReadEntitiesFromFile(buffer);

                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost, Route("upload-stream")]
        public async Task<IHttpActionResult> UploadStream()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsByteArrayAsync();
                //Do whatever you want with filename and its binaray data.
            }

            return Ok();
        }



        /*        
        public async Task<IHttpActionResult> UploadFile()
        {
            bool result = false;

            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            var filesReadToProvider = await Request.Content.ReadAsMultipartAsync();

            foreach (var stream in filesReadToProvider.Contents)
            {
                var file = await stream.ReadAsByteArrayAsync();

                using (var binaryReader = new BinaryReader(stream))
                {
                    var fileContent = binaryReader.ReadBytes((int)file.Length);
                    result = await _uploadService.AddFile(fileContent, file.FileName, System.IO.Path.GetExtension(file.FileName));
                }


            }
            return result;
        }
        
            [HttpPost] [ClaimsAuthorization]
        [Route("upload")] // mark
        public async Task<HttpResponseMessage> Upload(string title)
        {
            //var req = HttpContext.Current.Request.Form;

            //MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(Path.GetTempPath());
            //var result = await Request.Content.ReadAsMultipartAsync(provider);
            //var model = result.FormData["Metadata"];
            //var file = result.FileData;//How it will retriev the file data

            //if (file == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document cannot be null" });
            //if (file.Length == 0) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Document cannot be empty" });
            bool result = false;
            try
            {

                HttpResponseMessage response = new HttpResponseMessage();
                var httpRequest = HttpContext.Current.Request;

                if (httpRequest.Files.Count > 0)
                {
                    foreach (string file in httpRequest.Files)
                    {
                        using (Stream stream = file.OpenReadStream())
                        {
                            using (var binaryReader = new BinaryReader(stream))
                            {
                                var fileContent = binaryReader.ReadBytes((int)file.Length);
                                result = await _uploadService.AddFile(fileContent, file.FileName, System.IO.Path.GetExtension(file.FileName));
                            }
                        }
                    }
                }
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Document upload was successful" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "The was an error while uploading document" });

        }          
             
             
             
             
             */



        [HttpPost, Route("upload5")] // custom provider
        public async Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
            }

            // Read the file and form data.
            MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
            Task.Factory
                .StartNew(() => provider = Request.Content.ReadAsMultipartAsync(provider).Result,
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning, // guarantees separate thread
                    TaskScheduler.Default)
                .Wait();

            // Extract the fields from the form data.
            string description = provider.FormData["description"];
            int uploadType;
            if (!Int32.TryParse(provider.FormData["uploadType"], out uploadType))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
            }

            // Check if files are on the request.
            if (!provider.FileStreams.Any())
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            IList<string> uploadedFiles = new List<string>();
            foreach (KeyValuePair<string, Stream> file in provider.FileStreams)
            {
                string fileName = file.Key;
                Stream stream = file.Value;

                // Do something with the uploaded file
                //UploadManager.Upload(stream, fileName, uploadType, description);
                /*My UpdateManager.Upload function is a static method that just takes in the uploadType and determines what file uploader implementation 
                 * class to send the file to. For example, we have one for an certain type of excel formatted file. It creates an ExcelPackage(stream) and 
                 * then uses the ExcelPackage to read the rows data and upload into a database. Sorry, I can't give you details as it's very implementation specific*/

                // Keep track of the filename for the response
                uploadedFiles.Add(fileName);
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Successfully Uploaded: " + string.Join(", ", uploadedFiles));
        }

    }
}


public class MultipartFormDataMemoryStreamProvider : MultipartMemoryStreamProvider
{
    private readonly Collection<bool> _isFormData = new Collection<bool>();
    private readonly NameValueCollection _formData = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Stream> _fileStreams = new Dictionary<string, Stream>();

    public NameValueCollection FormData
    {
        get { return _formData; }
    }

    public Dictionary<string, Stream> FileStreams
    {
        get { return _fileStreams; }
    }

    public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
    {
        if (parent == null)
        {
            throw new ArgumentNullException("parent");
        }

        if (headers == null)
        {
            throw new ArgumentNullException("headers");
        }

        var contentDisposition = headers.ContentDisposition;
        if (contentDisposition == null)
        {
            throw new InvalidOperationException("Did not find required 'Content-Disposition' header field in MIME multipart body part.");
        }

        _isFormData.Add(String.IsNullOrEmpty(contentDisposition.FileName));

        return base.GetStream(parent, headers);
    }

    public override async Task ExecutePostProcessingAsync()
    {
        for (var index = 0; index < Contents.Count; index++)
        {
            HttpContent formContent = Contents[index];
            if (_isFormData[index])
            {
                // Field
                string formFieldName = UnquoteToken(formContent.Headers.ContentDisposition.Name) ?? string.Empty;
                string formFieldValue = await formContent.ReadAsStringAsync();
                FormData.Add(formFieldName, formFieldValue);
            }
            else
            {
                // File
                string fileName = UnquoteToken(formContent.Headers.ContentDisposition.FileName);
                Stream stream = await formContent.ReadAsStreamAsync();
                FileStreams.Add(fileName, stream);
            }
        }
    }

    private static string UnquoteToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return token;
        }

        if (token.StartsWith("\"", StringComparison.Ordinal) && token.EndsWith("\"", StringComparison.Ordinal) && token.Length > 1)
        {
            return token.Substring(1, token.Length - 2);
        }

        return token;
    }
}