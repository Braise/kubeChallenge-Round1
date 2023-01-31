using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calicot.Shared.Data;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

namespace Calicot.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        string imagesContainerName = "uploads";
        string thumbsContainerName = "thumbs";
        string cdnBaseUrl = "https://calicot.azureedge.net/";
        private readonly CalicotDB _context;
        private IWebHostEnvironment _env;
        private IBlobStorageService _blobStorageService;
        public ImageController(CalicotDB context, IWebHostEnvironment env, IBlobStorageService blobStorageService, IConfiguration configuration)
        {
            _context = context;
            _env = env;
            _blobStorageService = blobStorageService;
            imagesContainerName = configuration.GetSection("AppSettings:FullImageContainer").Value??"";
            thumbsContainerName = configuration.GetSection("AppSettings:ThumbnailContainer").Value??"";
            cdnBaseUrl = configuration.GetSection("AppSettings:cdnBaseUrl").Value??"";
        }

        public string GetImageCdnUrl(string fileName, bool tn) {
            var cdnUrl = this.cdnBaseUrl;
            var container = imagesContainerName;
            if(tn) {
                fileName = "tn_" + fileName;
                container = thumbsContainerName;
            }
            cdnUrl += $"{container}/{fileName}";
            return cdnUrl;
        }

        // GET: api/Image/5
        [HttpGet("{fileName}")]
        public IActionResult GetImage(string fileName, bool tn)
        {
            var container = imagesContainerName;
            if(tn) {
                fileName = "tn_" + fileName;
                container = thumbsContainerName;
            }

            // var imagePath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", fileName);

            
            // new FileExtensionContentTypeProvider().TryGetContentType(imagePath, out contentType);
            string contentType = "application/octet-stream";
            MemoryStream mem = _blobStorageService.GetBlobDataAsync(fileName, container).Result;
            if(mem != null) {
                contentType = _blobStorageService.GetContentTypeAsync(fileName, container).Result;
                mem.Position = 0;
                return new FileStreamResult(mem, contentType ?? "application/octet-stream");
            }

            // si l'image n'est pas trouvé on retourne une image "missing" placeholder.
            fileName = "missing_image.jpg";
            if(tn) {
                fileName = "tn_" + fileName;
            }

            mem = _blobStorageService.GetBlobDataAsync(fileName, container).Result;
            if(mem != null) {
                contentType = _blobStorageService.GetContentTypeAsync(fileName, container).Result;
                mem.Position = 0;
                return new FileStreamResult(mem, contentType ?? "application/octet-stream");
            }
            
            return new NotFoundResult();
            
        }
    }
}