using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.PixelFormats;
// using SixLabors.ImageSharp.Processing;

namespace Calicot.Function
{
    public class CreateThumb
    {
        [FunctionName("CreateThumb")]
        public void Run([BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] Stream myBlob,
                        string name,
                        [Blob("thumbs/tn_{name}", FileAccess.Write, Connection = "AzureWebJobsStorage")] Stream outputBlob,
                        ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var image = System.Drawing.Image.FromStream(myBlob);

            // using (Image<Rgba32> image = Image.FromStream(myBlob))
            // {
            //     image.Mutate(x => x
            //         .Resize(image.Width / 2, image.Height / 2)
            //         .Grayscale());
            //     //image.Save("bar.jpg"); // Automatic encoder selected based on extension.
            // }
            
            // calculate a 150px thumbnail
            int width;
            int height;
            if (image.Width > image.Height)
            {
                width = 150;
                height = 150 * image.Height / image.Width;
            }
            else
            {
                height = 150;
                width = 150 * image.Width / image.Height;
            }

            // generate the thumb
            var thumb = image.GetThumbnailImage(
                width,
                height,
                () => false,
                IntPtr.Zero);

            System.Drawing.Imaging.ImageFormat fmt = new System.Drawing.Imaging.ImageFormat(image.RawFormat.Guid);

            thumb.Save(
                    outputBlob,
                    fmt);

            log.LogInformation($"Blob uploads/{name} has been copied to thumbs/tn_{name}");

            // save it off to blob storage
            // using (var thumbStream = new MemoryStream())
            // {
            //     thumb.Save(
            //         thumbStream,
            //         System.Drawing.Imaging.ImageFormat.Jpeg);

            //     thumbStream.Position = 0; // reset;
                
            //     outputBlob = thumbStream;
            //     // BlobClient blob = container.GetBlobClient("tn_" + file);
            //     // await blob.UploadAsync(thumbStream, new BlobHttpHeaders { ContentType = "image/jpeg" });
            // }
        }
    }
}
