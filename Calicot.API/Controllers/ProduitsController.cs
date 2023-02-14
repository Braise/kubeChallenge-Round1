using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Calicot.Shared.Data;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using System.ComponentModel.DataAnnotations;

namespace Calicot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProduitsController : ControllerBase
    {
        string imagesContainerName = "uploads";
        string thumbsContainerName = "thumbs";
        string cdnBaseUrl = "https://calicot-cdn.azureedge.net/";
        private IBlobStorageService _blobStorageService;
        private ICosmosDbService _cosmosDbService;

        public ProduitsController(IBlobStorageService blobStorageService, ICosmosDbService cosmosDbService, IConfiguration configuration)
        {
            _blobStorageService = blobStorageService;
            _cosmosDbService = cosmosDbService;
            imagesContainerName = configuration.GetSection("AppSettings:FullImageContainer").Value??"";
            thumbsContainerName = configuration.GetSection("AppSettings:ThumbnailContainer").Value??"";
            cdnBaseUrl = configuration.GetSection("AppSettings:cdnBaseUrl").Value??"";

        }

        private string GetImageCdnUrl(string fileName, bool tn) {
            var cdnUrl = this.cdnBaseUrl;
            var container = imagesContainerName;
            if(tn) {
                fileName = "tn_" + fileName;
                container = thumbsContainerName;
            }
            cdnUrl += $"{container}/{fileName}";
            return cdnUrl;
        }

        // // GET: api/Produits
        // [HttpGet]
        // public async Task<ActionResult<IEnumerable<IProduit>>> GetProduit()
        // {
        //     var produits = await _cosmosDbService.GetProduitsAsync("");
        //     // foreach (var produit in produits)
        //     // {
        //     //     produit.ImageUrl = GetImageCdnUrl(produit.Image, false);
        //     //     produit.ImageThumbUrl = GetImageCdnUrl(produit.Image, true);
        //     // }
        //     return produits.ToList();
        // }

        // // GET: api/Produits/5
        // [HttpGet("{id}")]
        // public async Task<ActionResult<Produit>> GetProduit(string id)
        // {
        //     var produit = await _cosmosDbService.GetProduitAsync(id);
        //     if (produit == null)
        //     {
        //         return NotFound();
        //     }

        //     return (Produit)produit;
        // }

        // GET: api/Produits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProduitVM>>> GetProduitVM()
        {
            var produits = await _cosmosDbService.GetProduitsAsync("");
            var produitVMs = new List<ProduitVM> ();
            foreach (IProduit produit in produits)
            {
                var produitVM = new ProduitVM() {
                    Id = produit.Id,
                    Description = produit.Description,
                    Prix= produit.Prix,
                    Nom= produit.Nom,
                    Image = produit.Image,
                    ImageUrl = GetImageCdnUrl(produit.Image, false),
                    ImageThumbUrl = GetImageCdnUrl(produit.Image, true)
                };
                produitVMs.Add(produitVM);
            }
            return produitVMs;
        }

        // GET: api/Produits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProduitVM>> GetProduitVM(string id)
        {
            var produit = await _cosmosDbService.GetProduitAsync(id);
            var produitVM = new ProduitVM() {
                    Id = produit.Id,
                    Description = produit.Description,
                    Prix= produit.Prix,
                    Nom= produit.Nom,
                    Image = produit.Image,
                    ImageUrl = GetImageCdnUrl(produit.Image, false),
                    ImageThumbUrl = GetImageCdnUrl(produit.Image, true)
                };
            if (produitVM == null)
            {
                return NotFound();
            }

            return produitVM;
        }

        // PUT: api/Produits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduit(string id, Produit produit)
        {
            if (id != produit.Id)
            {
                return BadRequest();
            }

            var oldproduit = await _cosmosDbService.GetProduitAsync(id);
            if (oldproduit == null)
            {
                return NotFound();
            }

            // l'image a changé, on doit effacer l'ancienne image.
            if(produit.Image != oldproduit.Image) {
                _blobStorageService.DeleteBlobDataAsync(oldproduit.Image);
            }

            try
            {
                await _cosmosDbService.UpdateProduitAsync(id, produit);
                // await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProduitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Produits
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Produit>> PostProduit(Produit produit)
        {
            produit.Id = Guid.NewGuid().ToString();

            await _cosmosDbService.AddProduitAsync(produit);

            return CreatedAtAction("GetProduit", new { id = produit.Id }, produit);
        }

        // DELETE: api/Produits/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduit(string id)
        {
            var produit = await _cosmosDbService.GetProduitAsync(id);
            if (produit == null)
            {
                return NotFound();
            }

            _blobStorageService.DeleteBlobDataAsync(produit.Image);
            await _cosmosDbService.DeleteProduitAsync(id);

            return NoContent();
        }

        private bool ProduitExists(string id)
        {
            return _cosmosDbService.ExistProduitAsync(id).Result;
        }
    }
}
