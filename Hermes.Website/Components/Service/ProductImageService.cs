using Hermes.Website.Components.Service;
using Hermes.Data.Product.Models;

namespace Hermes.Website.Services
{
    // DTO pour les images côté client (pour éviter le conflit avec ImageProduct de l'API)
    public class ClientProductImage
    {
        public string DataUrl { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsUrl => Base64Data.StartsWith("http://") || Base64Data.StartsWith("https://");
    }

    public interface IProductImageService
    {
        Task<ImageProduct?> CreateImageAsync(string imageData, int? productId = null);
        Task<ImageProduct[]> CreateMultipleImagesAsync(List<ClientProductImage> images, int productId);
        Task<bool> DeleteImageAsync(int imageId);
        Task<ImageProduct[]> GetProductImagesAsync(int productId);
        Task<bool> SetPrimaryImageAsync(int productId, int imageId);
    }

    public class ProductImageService : IProductImageService
    {
        private readonly ApiService _apiService;
        private const string ENDPOINT = "Images";

        public ProductImageService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Crée une nouvelle image en base de données
        /// imageData peut être du Base64 ou une URL
        /// </summary>
        public async Task<ImageProduct?> CreateImageAsync(string imageData, int? productId = null)
        {
            try
            {
                var workspaceId = await _apiService.GetWorkspaceId();

                var imageDto = new ImageProduct
                {
                    ImageBase64 = imageData, // Base64 ou URL
                    ProductId = productId,
                    WorkspaceId = workspaceId,
                    CreatedAt = DateTime.UtcNow
                };

                return await _apiService.CreateItemAndReturnAsync<ImageProduct>(ENDPOINT, imageDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création de l'image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Crée plusieurs images pour un produit
        /// </summary>
        public async Task<ImageProduct[]> CreateMultipleImagesAsync(List<ClientProductImage> images, int productId)
        {
            var createdImages = new List<ImageProduct>();

            foreach (var image in images)
            {
                var createdImage = await CreateImageAsync(image.Base64Data, productId);
                if (createdImage != null)
                {
                    createdImages.Add(createdImage);
                }
            }

            return createdImages.ToArray();
        }

        /// <summary>
        /// Supprime une image
        /// </summary>
        public async Task<bool> DeleteImageAsync(int imageId)
        {
            try
            {
                return await _apiService.DeleteItemAsync(ENDPOINT, imageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de l'image {imageId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Récupère toutes les images d'un produit
        /// </summary>
        public async Task<ImageProduct[]> GetProductImagesAsync(int productId)
        {
            try
            {
                var allImages = await _apiService.GetArrayAsync<ImageProduct>(ENDPOINT);
                return allImages.Where(img => img.ProductId == productId).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des images du produit {productId}: {ex.Message}");
                return Array.Empty<ImageProduct>();
            }
        }

        /// <summary>
        /// Définit une image comme image principale d'un produit
        /// </summary>
        public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
        {
            try
            {
                return await _apiService.PutAsync($"Products/{productId}/primary-image/{imageId}", new { });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la définition de l'image principale: {ex.Message}");
                return false;
            }
        }
    }
}