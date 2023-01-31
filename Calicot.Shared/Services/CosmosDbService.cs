using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Calicot.Shared.Models;
using Calicot.Shared.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;

namespace Calicot.Shared.Services {
    public class CosmosDbService: ICosmosDbService
    {
        private Container _container;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }
        
        public async Task AddProduitAsync(Produit produit)
        {
            await this._container.CreateItemAsync<Produit>(produit, new PartitionKey(produit.Id));
        }

        public async Task DeleteProduitAsync(string id)
        {
            await this._container.DeleteItemAsync<Produit>(id, new PartitionKey(id));
        }

        public async Task<Produit> GetProduitAsync(string id)
        {
            try
            {
                ItemResponse<Produit> response = await this._container.ReadItemAsync<Produit>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                return default!;
            }

        }

        public async Task<Boolean> ExistProduitAsync(string id) {
            try
            {
                ItemResponse<Produit> response = await this._container.ReadItemAsync<Produit>(id, new PartitionKey(id));
                return true;
            }
            catch(CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { 
                return false;
            }
        }

        public async Task<IEnumerable<Produit>> GetProduitsAsync(string queryString)
        {
            var query = this._container.GetItemQueryIterator<Produit>();
            if(!string.IsNullOrWhiteSpace(queryString)){
                query = this._container.GetItemQueryIterator<Produit>(new QueryDefinition(queryString));
            }
            
            List<Produit> results = new List<Produit>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                
                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task UpdateProduitAsync(string id, Produit produit)
        {
            await this._container.UpsertItemAsync<Produit>(produit, new PartitionKey(id));
        }
    }
}
