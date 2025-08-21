namespace Hermes.Data.Product.Models
{
    public class Categorie
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public string Nom { get; set; } = string.Empty;
    }
}