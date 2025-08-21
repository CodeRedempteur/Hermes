using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Data.Product.Models
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Le nom du produit est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string Nom { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La description ne peut pas dépasser 1000 caractères")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Le prix est requis")]
        [Range(0.01, 999999.99, ErrorMessage = "Le prix doit être entre 0.01 et 999999.99")]
        public decimal Prix { get; set; }

        public bool IsPublished { get; set; } = false;

        // Foreign Keys optionnelles
        public int? ImageId { get; set; }
        public int? PlastiqueId { get; set; }
        public int? CategorieId { get; set; }
        public int? TagId { get; set; }
        public int? StockId { get; set; }
        public int? SeoId { get; set; }
    }

    /// <summary>
    /// Classe pour les statistiques des produits
    /// </summary>
    public class ProductStatistics
    {
        public int TotalProducts { get; set; }
        public int PublishedProducts { get; set; }
        public int DraftProducts { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public string AveragePriceFormatted => AveragePrice.ToString("C2");
        public string MinPriceFormatted => MinPrice.ToString("C2");
        public string MaxPriceFormatted => MaxPrice.ToString("C2");
        public double PublishedPercentage => TotalProducts > 0 ? (double)PublishedProducts / TotalProducts * 100 : 0;
    }
}
