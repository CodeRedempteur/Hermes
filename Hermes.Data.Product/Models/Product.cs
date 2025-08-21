using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Hermes.Data.Product.Models
{
    public class Product
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;

        // Propriétés mappées à la DB (françaises)
        public string Nom { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Prix { get; set; }
        public bool IsPublished { get; set; } = false; // Nouvelle propriété à ajouter à la DB

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime CreatedAt { get; set; }

        // Propriétés calculées (non mappées) pour compatibilité avec le contrôleur
        [NotMapped]
        public string Name
        {
            get => Nom;
            set => Nom = value;
        }

        [NotMapped]
        public decimal Price
        {
            get => Prix;
            set => Prix = value;
        }

        // Foreign Keys
        public int? ImageId { get; set; }
        public int? PlastiqueId { get; set; }
        public int? CategorieId { get; set; }
        public int? TagId { get; set; }
        public int? StockId { get; set; }
        public int? SeoId { get; set; }

        // Navigation properties pour relations simples
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ImageProduct? Image { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Plastique? Plastique { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Categorie? Categorie { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Tag? Tag { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Stock? Stock { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Seo? Seo { get; set; }
    }
}
