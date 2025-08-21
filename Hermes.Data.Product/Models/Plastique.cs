namespace Hermes.Data.Product.Models
{
    public class Plastique
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; } = 0;
        public string Nom { get; set; } = string.Empty;
        public decimal CoutGramme { get; set; }
    }
}
