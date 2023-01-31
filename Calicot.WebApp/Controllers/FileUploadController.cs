using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;

namespace Calicot.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        string imagesContainerName = "uploads";

        private IWebHostEnvironment _env = default!;
        private IBlobStorageService _blobStorageService = default!;

        public FileUploadController(IWebHostEnvironment env, IBlobStorageService blobStorageService)
        {
            _env = env;
            _blobStorageService = blobStorageService;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> PostFile()
        {
            var files = Request.Form.Files;

            foreach (var file in files)
            {
                if(file != null && file.ContentDisposition != null){
                    var filename = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName;
                    filename = (filename??"").Trim('"');
                
                    string mimeType = file.ContentType;

                    var azureFileName = await _blobStorageService.UploadFileToBlobAsync(filename, file, mimeType, imagesContainerName);
                    return Created(azureFileName, new { FileName = azureFileName});
                }
            }
            return Ok();
        }


    }
}