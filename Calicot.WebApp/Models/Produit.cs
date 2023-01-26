using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Calicot.WebApp.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("Produits")]
    public class Produit : IProduit
    {
        [Key]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = default!;

        [JsonProperty(PropertyName = "nom")]
        public string Nom { get; set; } = default!;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = default!;

        [JsonProperty(PropertyName = "prix")]
        public float Prix { get; set; } = default!;

        [JsonProperty(PropertyName = "image")]
        public string Image {get; set;} = default!;

    }

    public class ProduitVM : IProduit
    {
        [Key]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = default!;

        [JsonProperty(PropertyName = "nom")]
        public string Nom { get; set; } = default!;

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = default!;

        [JsonProperty(PropertyName = "prix")]
        public float Prix { get; set; } = default!;

        [JsonProperty(PropertyName = "image")]
        public string Image {get; set;} = default!;

        [JsonProperty(PropertyName = "imageurl")]
        public virtual string ImageUrl {get; set;} = default!;

        [JsonProperty(PropertyName = "imagethumburl")]
        public virtual string ImageThumbUrl {get; set;} = default!;
    }

    public interface IProduit
    {
        string Id { get; set; }
        string Nom { get; set; }
        string Description { get; set; }
        float Prix { get; set; }
        string Image {get; set;}
    }
}