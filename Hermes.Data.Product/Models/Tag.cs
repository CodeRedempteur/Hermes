namespace Hermes.Data.Product.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public string Nom { get; set; } = string.Empty;
    }
}