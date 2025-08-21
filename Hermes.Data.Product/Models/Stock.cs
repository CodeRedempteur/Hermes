namespace Hermes.Data.Product.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public string TypeStock { get; set; } = string.Empty;
    }
}