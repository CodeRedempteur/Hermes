using System.Text.Json.Serialization;

namespace Hermes.Data.Product.Models
{
    public class ImageProduct
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public int? ProductId { get; set; } // Déjà présent - correct
        public string? ImageBase64 { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public Product? Product { get; set; }
    }
}