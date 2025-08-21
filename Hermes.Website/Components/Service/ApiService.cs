using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
using System.Text.Json;

namespace Hermes.Website.Components.Service
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private int _currentWorkspaceId;

        // Options JSON réutilisables
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly JsonSerializerOptions _debugJsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService(
            HttpClient httpClient,
            IConfiguration configuration,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7247/api/");
        }

        public void InitializeToken()
        {
            // TODO: Implémenter l'authentification si nécessaire
        }

        public async Task<int> GetWorkspaceId()
        {
            return _currentWorkspaceId > 0 ? _currentWorkspaceId : 1;
        }

        public void SetCurrentWorkspace(int workspaceId)
        {
            _currentWorkspaceId = workspaceId;
        }

        private async Task AddHeaders()
        {
            // TODO: Ajouter les headers d'authentification si nécessaire
            await Task.CompletedTask;
        }

        // ===== MÉTHODES GET =====

        /// <summary>
        /// Récupère une liste d'éléments
        /// </summary>
        public async Task<T[]> GetArrayAsync<T>(string endpoint)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GET {endpoint} failed with status: {response.StatusCode}");
                    return Array.Empty<T>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<T[]>(content, _jsonOptions);
                return result ?? Array.Empty<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetArrayAsync for {endpoint}: {ex.Message}");
                return Array.Empty<T>();
            }
        }

        /// <summary>
        /// Récupère un élément par son ID
        /// </summary>
        public async Task<T?> GetItemByIdAsync<T>(string endpoint, int id, Dictionary<string, string>? queryParams = null)
        {
            try
            {
                await AddHeaders();
                string url = $"{endpoint}/{id}";

                if (queryParams?.Count > 0)
                {
                    url += "?" + string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GET {url} failed with status: {response.StatusCode}");
                    return default;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetItemByIdAsync for {endpoint}/{id}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Récupère un élément unique
        /// </summary>
        public async Task<T?> GetItemAsync<T>(string endpoint)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GET {endpoint} failed with status: {response.StatusCode}");
                    return default;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetItemAsync for {endpoint}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Récupère la réponse brute
        /// </summary>
        public async Task<string> GetRawResponseAsync(string endpoint)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetRawResponseAsync for {endpoint}: {ex.Message}");
                return string.Empty;
            }
        }

        // ===== MÉTHODES POST =====

        /// <summary>
        /// Crée un élément et retourne l'objet créé
        /// </summary>
        public async Task<T?> CreateItemAndReturnAsync<T>(string endpoint, object data)
        {
            try
            {
                await AddHeaders();

                string jsonData = JsonSerializer.Serialize(data, _debugJsonOptions);
                Console.WriteLine($"POST to {endpoint}:");
                Console.WriteLine(jsonData);

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                Console.WriteLine($"Response status: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Success response: {responseContent}");
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
                else
                {
                    Console.WriteLine($"Error response: {responseContent}");
                    return default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateItemAndReturnAsync for {endpoint}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Crée un élément (retourne boolean)
        /// </summary>
        public async Task<bool> CreateItemAsync<T>(string endpoint, T item)
        {
            try
            {
                await AddHeaders();

                var json = JsonSerializer.Serialize(item, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"POST {endpoint} failed. Status: {response.StatusCode}, Error: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in CreateItemAsync for {endpoint}: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTHODES PUT =====

        /// <summary>
        /// Met à jour un élément
        /// </summary>
        public async Task<bool> UpdateItemAsync<T>(string endpoint, int id, T item)
        {
            try
            {
                await AddHeaders();
                var json = JsonSerializer.Serialize(item, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{endpoint}/{id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PUT {endpoint}/{id} failed. Status: {response.StatusCode}, Error: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in UpdateItemAsync for {endpoint}/{id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Met à jour avec endpoint personnalisé
        /// </summary>
        public async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                await AddHeaders();
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"PUT {endpoint} failed. Status: {response.StatusCode}, Error: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PutAsync for {endpoint}: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTHODES DELETE =====

        /// <summary>
        /// Supprime un élément par ID
        /// </summary>
        public async Task<bool> DeleteItemAsync(string endpoint, int id)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DELETE {endpoint}/{id} failed. Status: {response.StatusCode}, Error: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteItemAsync for {endpoint}/{id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Supprime avec endpoint personnalisé
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.DeleteAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DELETE {endpoint} failed. Status: {response.StatusCode}, Error: {errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in DeleteAsync for {endpoint}: {ex.Message}");
                return false;
            }
        }

        // ===== MÉTHODES UTILITAIRES =====

        /// <summary>
        /// Récupère le nombre d'éléments
        /// </summary>
        public async Task<int> GetCountAsync(string endpoint)
        {
            try
            {
                await AddHeaders();
                var response = await _httpClient.GetAsync($"{endpoint}/count");

                if (!response.IsSuccessStatusCode)
                {
                    return 0;
                }

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<int>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetCountAsync for {endpoint}: {ex.Message}");
                return 0;
            }
        }
    }
}