namespace Hermes.Data.Product.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public Product? Product { get; set; }
    }
}