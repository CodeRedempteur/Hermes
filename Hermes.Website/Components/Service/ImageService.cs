using Microsoft.AspNetCore.Components.Forms;

namespace Hermes.Website.Components.Service
{
    public interface IImageService
    {
        Task<string> ConvertFileToBase64Async(IBrowserFile file, int maxSize = 2 * 1024 * 1024);
        Task<string> DownloadImageAsBase64Async(string url, int maxSize = 2 * 1024 * 1024);
        string GetImageDataUrl(string base64String);
        bool IsUrl(string imageData);
        string GetDisplayUrl(string imageData);
    }

    public class ImageService : IImageService
    {
        private readonly HttpClient _httpClient;

        public ImageService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Convertit un fichier téléchargé en chaîne Base64
        /// </summary>
        public async Task<string> ConvertFileToBase64Async(IBrowserFile file, int maxSize = 2 * 1024 * 1024)
        {
            if (file == null) return string.Empty;

            // Vérifier la taille du fichier
            if (file.Size > maxSize)
                throw new Exception($"La taille de l'image dépasse la limite maximale de {maxSize / 1024 / 1024} MB");

            try
            {
                using var stream = file.OpenReadStream(maxSize);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                byte[] fileBytes = ms.ToArray();
                return Convert.ToBase64String(fileBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors de la conversion de l'image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Télécharge une image à partir d'une URL et la convertit en Base64
        /// </summary>
        public async Task<string> DownloadImageAsBase64Async(string url, int maxSize = 2 * 1024 * 1024)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            // Valider le format de l'URL
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                throw new Exception("Format d'URL invalide");
            }

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Vérifier le type de contenu
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType == null || !contentType.StartsWith("image/"))
                {
                    throw new Exception("Le fichier n'est pas une image valide");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                // Vérifier la taille
                if (imageBytes.Length > maxSize)
                {
                    throw new Exception($"L'image est trop volumineuse (maximum {maxSize / 1024 / 1024} MB)");
                }

                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erreur lors du téléchargement de l'image: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retourne l'URL de données pour afficher une image Base64
        /// </summary>
        public string GetImageDataUrl(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return string.Empty;
            return $"data:image/png;base64,{base64String}";
        }

        /// <summary>
        /// Détermine si la chaîne est une URL ou du Base64
        /// </summary>
        public bool IsUrl(string imageData)
        {
            if (string.IsNullOrEmpty(imageData)) return false;
            return imageData.StartsWith("http://") || imageData.StartsWith("https://");
        }

        /// <summary>
        /// Retourne l'URL d'affichage appropriée selon le type de données
        /// </summary>
        public string GetDisplayUrl(string imageData)
        {
            if (string.IsNullOrEmpty(imageData)) return string.Empty;

            if (IsUrl(imageData))
            {
                return imageData; // C'est déjà une URL
            }
            else
            {
                return GetImageDataUrl(imageData); // Convertir le Base64 en data URL
            }
        }
    }
}