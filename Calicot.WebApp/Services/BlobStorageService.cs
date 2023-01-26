using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Diagnostics;

namespace Calicot.WebApp.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        string connectionString = string.Empty;
        string imagesContainerName = "uploads";
        string thumbsContainerName = "thumbs";

        public BlobStorageService(IConfiguration configuration)
        {
            this.connectionString = configuration.GetSection("CalicotImages").GetSection("ConnectionString").Value??"";
        }

        public async Task<string> GetContentTypeAsync(string fileName, string containerName) {
            var container = await getContainerAsync(containerName);

            // get block blob reférence    
            BlobClient blob = container.GetBlobClient(fileName);

            if (await blob.ExistsAsync()) {
                var properties = blob.GetProperties(null, default);

                return  properties.Value.ContentType;
            }
            return default!;

        }

        private async Task<BlobContainerClient> getContainerAsync(string containerName) {
            BlobContainerClient container = new BlobContainerClient(this.connectionString, containerName);
            if(!await container.ExistsAsync()) {
                container.Create();
            }
            return container;
        }

        public async Task<MemoryStream> GetBlobDataAsync(string fileName, string containerName) {
            var container = await getContainerAsync(containerName);

            // get block blob reférence    
            BlobClient blob = container.GetBlobClient(fileName);

            if (await blob.ExistsAsync()) {
                MemoryStream mem = new MemoryStream();  
                await blob.DownloadToAsync(mem).ConfigureAwait(false);
                return mem;
            }

            return default!;
        }

        public async Task<string> UploadFileStreamToBlobAsync(string fileName, FileStream file, string fileMimeType, string containerName)
        {
                var container = await getContainerAsync(containerName);

                string newfileName = this.GenerateFileName(fileName);

                // get block blob reférence    
                BlobClient blob = container.GetBlobClient(newfileName);
                using (var target = new MemoryStream())  
                {  
                    await file.CopyToAsync(target);
                    target.Position = 0;
                    await blob.UploadAsync(target, new BlobHttpHeaders { ContentType = fileMimeType });
                }

                // CreateThumb(blob.Name);
                return blob.Name;
        }

        public async Task<string> UploadMemoryStreamToBlobAsync(string fileName, MemoryStream ms, string fileMimeType, string containerName)
        {
                var container = await getContainerAsync(containerName);

                string newfileName = this.GenerateFileName(fileName);

                // get block blob reférence    
                BlobClient blob = container.GetBlobClient(newfileName);
                ms.Position = 0;
                await blob.UploadAsync(ms, new BlobHttpHeaders { ContentType = fileMimeType });
                // CreateThumb(blob.Name);
                return blob.Name;
        }

        public async Task<string> UploadFileToBlobAsync(string strFileName, IFormFile file, string fileMimeType, string containerName)
        {
                var container = await getContainerAsync(containerName);

                string fileName = this.GenerateFileName(strFileName);

                if (fileName != null && file != null)
                {
                    BlobClient blob = container.GetBlobClient(fileName);
                    using (var target = new MemoryStream())  
                    {  
                        await file.OpenReadStream().CopyToAsync(target);
                        target.Position = 0;
                        await blob.UploadAsync(target, new BlobHttpHeaders { ContentType = fileMimeType });
                    }
                    // CreateThumb(blob.Name);
                    return blob.Name;
                }
                return "";
        }

        public async void DeleteBlobDataAsync(string fileName)
        {
            var container = await getContainerAsync(imagesContainerName);

            // get block blob reférence
            BlobClient blob = container.GetBlobClient(fileName);
            if (await blob.ExistsAsync()) {
                // delete blob from container
                await blob.DeleteAsync();
            }

            // Delete Thumbsnail too if it exist.
            var thumbs = await getContainerAsync(thumbsContainerName);
            BlobClient blobthumbs = thumbs.GetBlobClient("tn_" + fileName);
            if (await blobthumbs.ExistsAsync()) {
                // delete blob from container
                await blobthumbs.DeleteAsync();
            }

        }


        private string GenerateFileName(string fileName)
        {
            string strFileName = string.Empty;
            string[] strName = fileName.Split('.');
            // strFileName = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd") + "/" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[strName.Length - 1];
            strFileName = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[strName.Length - 1];
            return strFileName;
        }

        // private async void CreateThumb(string file)
        // {
        //     var container = await getContainerAsync(containerName);

        //     using (var ms = this.GetBlobDataAsync(file).Result)
        //     {
        //         var image = Image.FromStream(ms);

        //         // calculate a 150px thumbnail
        //         int width;
        //         int height;
        //         if (image.Width > image.Height)
        //         {
        //             width = 150;
        //             height = 150 * image.Height / image.Width;
        //         }
        //         else
        //         {
        //             height = 150;
        //             width = 150 * image.Width / image.Height;
        //         }

        //         // generate the thumb
        //         var thumb = image.GetThumbnailImage(
        //             width,
        //             height,
        //             () => false,
        //             IntPtr.Zero);

        //         // save it off to blob storage
        //         using (var thumbStream = new MemoryStream())
        //         {
        //             thumb.Save(
        //                 thumbStream,
        //                 System.Drawing.Imaging.ImageFormat.Jpeg);

        //             thumbStream.Position = 0; // reset;

        //             BlobClient blob = container.GetBlobClient("tn_" + file);
        //             await blob.UploadAsync(thumbStream, new BlobHttpHeaders { ContentType = "image/jpeg" });
        //         }

        //         Trace.TraceInformation("Thumbs for {0} created", file);
        //     }
        // }
    }
}