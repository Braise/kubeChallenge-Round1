using System;
using System.Collections.Generic;
    using System.Threading.Tasks;
    using Calicot.Shared.Models;


namespace Calicot.Shared.Services {

    public interface ICosmosDbService
    {
        Task<IEnumerable<Produit>> GetProduitsAsync(string query);
        Task<Produit> GetProduitAsync(string id);
        Task<Boolean> ExistProduitAsync(string id);
        Task AddProduitAsync(Produit produit);
        Task UpdateProduitAsync(string id, Produit produit);
        Task DeleteProduitAsync(string id);
    }
}