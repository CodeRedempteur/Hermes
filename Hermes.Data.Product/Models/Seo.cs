namespace Hermes.Data.Product.Models
{
    public class Seo
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public int? ProductId { get; set; } // Nouvelle propriété nécessaire
        public string Titre { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        public Product? Product { get; set; }
    }
}