using Calicot.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;
using Calicot.Shared.Data;
using Calicot.Shared.Services;


namespace Calicot.Shared;

public class DataGenerator
{
    public static async void Initialize(IServiceProvider serviceProvider, IBlobStorageService blobStorageService, IWebHostEnvironment env, ICosmosDbService cosmosDbService)
    {
        // using (var context = new CalicotDB(serviceProvider.GetRequiredService<DbContextOptions<CalicotDB>>()))
        // {
        //     // await context.Database.EnsureDeletedAsync();
        //     await context.Database.EnsureCreatedAsync();

            

            // Est-ce que la BD a déjà été seedé?
            // var produits = await context.Produits.ToListAsync();
            var produits = await cosmosDbService.GetProduitsAsync("");
            if (produits.Any())
            {
                return;   // Data was already seeded
            }

            var image1 = uploadImageFile("0000065_promo-hoodie-homme_550.png", blobStorageService, env);
            var image2 = uploadImageFile("0000063_hoodie-femme_550.png", blobStorageService, env);
            var image3 = uploadImageFile("0000018_gants-tactiles_550.jpeg", blobStorageService, env);
            var image4 = uploadImageFile("socks.jpeg", blobStorageService, env);
            var missing_image = uploadImageFile("missing_image.jpg", blobStorageService, env);


            // context.AddRange(
                await cosmosDbService.AddProduitAsync(new Produit
                {
                    Id = Guid.NewGuid().ToString(),
                    Nom = "Hoodie Cofomo Homme",
                    Description = "hoodie 60% coton/40% polyester. Hood, decorative zipperpull impeint tone on tone on right sleeve, embroiderye O left side",
                    Prix = 95.0f,
                    Image = image1
                });
                await cosmosDbService.AddProduitAsync(new Produit
                {
                    Id = Guid.NewGuid().ToString(),
                    Nom = "Hoodie Cofomo Femme",
                    Description = "hoodie 60% coton/40% polyester. Hood, decorative zipperpull impeint tone on tone on right sleeve, embroiderye O left side",
                    Prix = 95.0f,
                    Image = image2
                });
                await cosmosDbService.AddProduitAsync(new Produit
                {
                    Id = Guid.NewGuid().ToString(),
                    Nom = "Acrylic touch screen gloves",
                    Description = "Acrylic touch screen gloves, for smartphone or tablet. Made of acrylic. One size fits all.",
                    Prix = 14.75f,
                    Image = image3
                });
                await cosmosDbService.AddProduitAsync(new Produit
                {
                    Id = Guid.NewGuid().ToString(),
                    Nom = "Bas de laine",
                    Description = "Bas de laine super chaud 74% acrylic, 19% nylon, 6% wool, 1% spandex",
                    Prix = 19.95f,
                    Image = image4
                });
            // );

            // await context.SaveChangesAsync();
        // }
    }

    private static string uploadImageFile(string fileName, IBlobStorageService blobStorageService, IWebHostEnvironment env) {
        var savePath = Path.Combine(env.ContentRootPath, "wwwroot", "uploads", fileName);
        new FileExtensionContentTypeProvider().TryGetContentType(savePath, out string? contentType);
        if(!string.IsNullOrEmpty( contentType )){
            using (FileStream file = new FileStream(savePath, FileMode.Open))
            {
                return blobStorageService.UploadFileStreamToBlobAsync(fileName, file, contentType, "uploads").Result;
            }
        }
        return default!;
        // byte[] fileData = File.ReadAllBytes(savePath);
        // return blobStorageService.UploadFileToBlob(fileName, fileData, contentType);
    }

}

