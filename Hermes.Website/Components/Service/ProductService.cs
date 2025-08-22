using Hermes.Website.Components.Service;
using Hermes.Data.Product.Models; // Utiliser les modèles de l'API

namespace Hermes.Website.Services
{
    public class ProductService
    {
        private readonly ApiService _apiService;
        private const string ENDPOINT = "Products";

        public ProductService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ===== OPÉRATIONS CRUD =====

        /// <summary>
        /// Récupère tous les produits
        /// </summary>
        public async Task<Product[]> GetAllProductsAsync()
        {
            try
            {
                var products = await _apiService.GetArrayAsync<Product>(ENDPOINT);
                return products ?? Array.Empty<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des produits: {ex.Message}");
                return Array.Empty<Product>();
            }
        }
        // Ajoutez cette méthode à votre ProductService :

        /// <summary>
        /// Récupère une image par son ID
        /// </summary>
        public async Task<ImageProduct?> GetImageByIdAsync(int imageId)
        {
            try
            {
                return await _apiService.GetItemByIdAsync<ImageProduct>("Images", imageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'image {imageId}: {ex.Message}");
                return null;
            }
        }
        /// <summary>
        /// Récupère un produit par son ID
        /// </summary>
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _apiService.GetItemByIdAsync<Product>(ENDPOINT, id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du produit {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Récupère un produit avec ses détails complets
        /// </summary>
        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            try
            {
                return await _apiService.GetItemByIdAsync<Product>($"{ENDPOINT}", id,
                    new Dictionary<string, string> { { "includeDetails", "true" } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des détails du produit {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Crée un nouveau produit
        /// </summary>
        public async Task<Product?> CreateProductAsync(ProductCreateDto productDto)
        {
            try
            {
                var workspaceId = await _apiService.GetWorkspaceId();

                var product = new Product
                {
                    Nom = productDto.Nom,
                    Description = productDto.Description,
                    Prix = productDto.Prix,
                    IsPublished = productDto.IsPublished,
                    WorkspaceId = workspaceId,
                    ImageId = productDto.ImageId,
                    PlastiqueId = productDto.PlastiqueId,
                    CategorieId = productDto.CategorieId,
                    TagId = productDto.TagId,
                    StockId = productDto.StockId,
                    SeoId = productDto.SeoId
                };

                return await _apiService.CreateItemAndReturnAsync<Product>(ENDPOINT, product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création du produit: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Met à jour un produit existant
        /// </summary>
        public async Task<bool> UpdateProductAsync(int id, ProductCreateDto productDto)
        {
            try
            {
                var existingProduct = await GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    Console.WriteLine($"Produit {id} non trouvé pour mise à jour");
                    return false;
                }

                // Mettre à jour les propriétés
                existingProduct.Nom = productDto.Nom;
                existingProduct.Description = productDto.Description;
                existingProduct.Prix = productDto.Prix;
                existingProduct.IsPublished = productDto.IsPublished;
                existingProduct.ImageId = productDto.ImageId;
                existingProduct.PlastiqueId = productDto.PlastiqueId;
                existingProduct.CategorieId = productDto.CategorieId;
                existingProduct.TagId = productDto.TagId;
                existingProduct.StockId = productDto.StockId;
                existingProduct.SeoId = productDto.SeoId;

                // Nettoyer les propriétés de navigation pour l'envoi
                existingProduct.Image = null;
                existingProduct.Categorie = null;
                existingProduct.Seo = null;
                existingProduct.Plastique = null;
                existingProduct.Tag = null;
                existingProduct.Stock = null;

                return await _apiService.UpdateItemAsync(ENDPOINT, id, existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour du produit {id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime un produit
        /// </summary>
        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                return await _apiService.DeleteItemAsync(ENDPOINT, id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression du produit {id}: {ex.Message}");
                return false;
            }
        }

        // ===== OPÉRATIONS SPÉCIALES =====

        /// <summary>
        /// Publie un produit
        /// </summary>
        public async Task<bool> PublishProductAsync(int id)
        {
            try
            {
                return await _apiService.PutAsync($"{ENDPOINT}/{id}/publish", new { });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la publication du produit {id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Dépublie un produit
        /// </summary>
        public async Task<bool> UnpublishProductAsync(int id)
        {
            try
            {
                return await _apiService.PutAsync($"{ENDPOINT}/{id}/unpublish", new { });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la dépublication du produit {id}: {ex.Message}");
                return false;
            }
        }

        // ===== OPÉRATIONS DE RECHERCHE ET FILTRAGE =====

        /// <summary>
        /// Recherche des produits par nom
        /// </summary>
        public async Task<Product[]> SearchProductsByNameAsync(string searchTerm)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                return allProducts
                    .Where(p => p.Nom.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche de produits: {ex.Message}");
                return Array.Empty<Product>();
            }
        }

        /// <summary>
        /// Filtre les produits par statut de publication
        /// </summary>
        public async Task<Product[]> GetProductsByStatusAsync(bool isPublished)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                return allProducts.Where(p => p.IsPublished == isPublished).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du filtrage des produits: {ex.Message}");
                return Array.Empty<Product>();
            }
        }

        /// <summary>
        /// Filtre les produits par catégorie
        /// </summary>
        public async Task<Product[]> GetProductsByCategoryAsync(int categoryId)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                return allProducts.Where(p => p.CategorieId == categoryId).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du filtrage des produits par catégorie: {ex.Message}");
                return Array.Empty<Product>();
            }
        }

        /// <summary>
        /// Récupère les produits récents
        /// </summary>
        public async Task<Product[]> GetRecentProductsAsync(int count = 10)
        {
            try
            {
                var allProducts = await GetAllProductsAsync();
                return allProducts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(count)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des produits récents: {ex.Message}");
                return Array.Empty<Product>();
            }
        }

        // ===== STATISTIQUES =====

        /// <summary>
        /// Récupère le nombre total de produits
        /// </summary>
        public async Task<int> GetProductsCountAsync()
        {
            try
            {
                return await _apiService.GetCountAsync(ENDPOINT);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du nombre de produits: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Récupère les statistiques des produits
        /// </summary>
        public async Task<ProductStatistics> GetProductStatisticsAsync()
        {
            try
            {
                var allProducts = await GetAllProductsAsync();

                return new ProductStatistics
                {
                    TotalProducts = allProducts.Length,
                    PublishedProducts = allProducts.Count(p => p.IsPublished),
                    DraftProducts = allProducts.Count(p => !p.IsPublished),
                    AveragePrice = allProducts.Any() ? allProducts.Average(p => p.Prix) : 0,
                    MinPrice = allProducts.Any() ? allProducts.Min(p => p.Prix) : 0,
                    MaxPrice = allProducts.Any() ? allProducts.Max(p => p.Prix) : 0
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des statistiques: {ex.Message}");
                return new ProductStatistics();
            }
        }
    }
}