using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.media;
using System.IO;

namespace FintrakBanking.APICore.Controllers
{

    [EnableCors("AllDomain")]
    [Route("api/v1/media")]
    public class ImageUploadController : Controller
    {
        IMediaRepository _uploadService;
        public ImageUploadController(IMediaRepository uploadService)
        {
            this._uploadService = uploadService;
        }

        [HttpGet("document")]
        public IActionResult GetDocument([FromQuery] int id)
        {
            return Ok(new { result = _uploadService.GetDocumentById(id) });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file,string title)
        {
            var req = Request.HttpContext.Request.Form;

            if (file == null) return Ok(new { success = false, message = "Document cannot be null" });
            if (file.Length == 0) return Ok(new { success = false, message = "Document cannot be empty" });
            bool result = false;
            try
            {
                using (Stream stream = file.OpenReadStream())
                {
                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var fileContent = binaryReader.ReadBytes((int)file.Length);
                        result = await _uploadService.AddFile(fileContent, file.FileName, System.IO.Path.GetExtension(file.FileName));
                    }
                }
                if (result)
                {
                    return Ok(new { success = true, message = "Document upload was successful" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }

            return Ok(new { success = false, message = "The was an error while uploading document" });

        }
    }
}