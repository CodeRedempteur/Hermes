using Microsoft.JSInterop;
using System.Text.Json;

namespace Hermes.Website.Services
{
    public interface ICartService
    {
        Task<List<CartItem>> GetCartItemsAsync();
        Task AddToCartAsync(int productId);
        Task UpdateQuantityAsync(int productId, int quantity);
        Task RemoveFromCartAsync(int productId);
        Task ClearCartAsync();
        Task<int> GetCartItemCountAsync(); // Nombre d'articles uniques
        Task<int> GetCartTotalQuantityAsync(); // Quantité totale
        event Action? OnCartChanged;
    }

    public class CartService : ICartService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CART_KEY = "eternal_bears_cart";
        private List<CartItem> _cartItems = new();
        private bool _isInitialized = false;

        public event Action? OnCartChanged;

        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Vérifie si JavaScript est disponible (pas en prerendering)
        private async Task<bool> IsJavaScriptAvailableAsync()
        {
            try
            {
                // Test simple pour vérifier si JS est disponible
                await _jsRuntime.InvokeAsync<string>("eval", "''");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CartItem>> GetCartItemsAsync()
        {
            try
            {
                // Vérifier si JavaScript est disponible
                if (!await IsJavaScriptAvailableAsync())
                {
                    Console.WriteLine("JavaScript non disponible - retour liste vide");
                    return new List<CartItem>();
                }

                // TEST: Vérifier toutes les clés du localStorage
                var allKeysScript = "Object.keys(localStorage)";
                var allKeys = await _jsRuntime.InvokeAsync<string[]>("eval", allKeysScript);
                Console.WriteLine($"Toutes les clés localStorage: [{string.Join(", ", allKeys ?? new string[0])}]");

                // TEST: Vérifier spécifiquement notre clé
                var cartJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", CART_KEY);
                Console.WriteLine($"JSON récupéré du localStorage avec clé '{CART_KEY}': {cartJson ?? "null"}");

                // TEST: Vérifier avec eval direct
                var directCheck = await _jsRuntime.InvokeAsync<string>("eval", $"localStorage.getItem('{CART_KEY}')");
                Console.WriteLine($"Vérification directe via eval: {directCheck ?? "null"}");

                if (!string.IsNullOrEmpty(cartJson))
                {
                    var deserializeOptions = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        PropertyNameCaseInsensitive = true
                    };

                    var items = JsonSerializer.Deserialize<List<CartItem>>(cartJson, deserializeOptions) ?? new List<CartItem>();
                    Console.WriteLine($"Items désérialisés: {items.Count}");

                    // Log détaillé de chaque item
                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        Console.WriteLine($"Item {i}: ProductId={item.ProductId}, Quantity={item.Quantity}, AddedAt={item.AddedAt}");
                    }

                    // Filtrer les items avec des ProductId invalides
                    _cartItems = items.Where(item => item.ProductId > 0).ToList();
                    Console.WriteLine($"Items après filtrage (ProductId > 0): {_cartItems.Count}");

                    // Si on a supprimé des items invalides, sauvegarder la liste nettoyée
                    if (_cartItems.Count != items.Count)
                    {
                        Console.WriteLine($"Supprimé {items.Count - _cartItems.Count} items avec ProductId invalide");

                        // Log des items supprimés
                        var removedItems = items.Where(item => item.ProductId <= 0).ToList();
                        foreach (var removed in removedItems)
                        {
                            Console.WriteLine($"Item supprimé: ProductId={removed.ProductId}, Quantity={removed.Quantity}");
                        }

                        await SaveCartAsync();
                    }
                }
                else
                {
                    _cartItems = new List<CartItem>();
                }

                _isInitialized = true;
                return _cartItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du panier: {ex.Message}");
                return new List<CartItem>();
            }
        }

        public async Task AddToCartAsync(int productId)
        {
            try
            {
                // Validation de l'ID du produit
                if (productId <= 0)
                {
                    Console.WriteLine($"ProductId invalide: {productId}");
                    throw new ArgumentException("ProductId doit être supérieur à 0");
                }

                // Vérifier si JavaScript est disponible
                if (!await IsJavaScriptAvailableAsync())
                {
                    Console.WriteLine("JavaScript non disponible - impossible d'ajouter au panier");
                    throw new InvalidOperationException("Service non disponible pendant le prerendering");
                }

                if (!_isInitialized)
                {
                    await GetCartItemsAsync();
                }

                var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == productId);

                if (existingItem != null)
                {
                    // L'article existe déjà, on augmente la quantité
                    existingItem.Quantity += 1;
                    existingItem.UpdatedAt = DateTime.Now;
                    Console.WriteLine($"Quantité augmentée pour le produit {productId}: {existingItem.Quantity}");
                }
                else
                {
                    // Nouvel article
                    var newItem = new CartItem
                    {
                        ProductId = productId,
                        Quantity = 1,
                        AddedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _cartItems.Add(newItem);
                    Console.WriteLine($"Nouveau produit ajouté au panier: {productId}");
                }

                await SaveCartAsync();
                OnCartChanged?.Invoke();
                Console.WriteLine($"Panier mis à jour - Total items: {_cartItems.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout au panier: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateQuantityAsync(int productId, int quantity)
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    throw new InvalidOperationException("Service non disponible pendant le prerendering");
                }

                if (!_isInitialized)
                {
                    await GetCartItemsAsync();
                }

                var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    if (quantity <= 0)
                    {
                        await RemoveFromCartAsync(productId);
                    }
                    else
                    {
                        item.Quantity = quantity;
                        item.UpdatedAt = DateTime.Now;
                        await SaveCartAsync();
                        OnCartChanged?.Invoke();
                        Console.WriteLine($"Quantité mise à jour pour le produit {productId}: {quantity}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de la quantité: {ex.Message}");
                throw;
            }
        }

        public async Task RemoveFromCartAsync(int productId)
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    throw new InvalidOperationException("Service non disponible pendant le prerendering");
                }

                if (!_isInitialized)
                {
                    await GetCartItemsAsync();
                }

                var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
                if (item != null)
                {
                    _cartItems.Remove(item);
                    await SaveCartAsync();
                    OnCartChanged?.Invoke();
                    Console.WriteLine($"Produit {productId} supprimé du panier");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression du panier: {ex.Message}");
                throw;
            }
        }

        public async Task ClearCartAsync()
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    throw new InvalidOperationException("Service non disponible pendant le prerendering");
                }

                _cartItems.Clear();
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CART_KEY);
                OnCartChanged?.Invoke();
                Console.WriteLine("Panier vidé");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vidange du panier: {ex.Message}");
                throw;
            }
        }

        public async Task<int> GetCartItemCountAsync()
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    return 0;
                }

                var items = await GetCartItemsAsync();

                // Compter les articles uniques (distinct par ProductId)
                var uniqueItems = items.GroupBy(x => x.ProductId).Count();
                Console.WriteLine($"Compteur panier - Articles uniques: {uniqueItems}, Quantité totale: {items.Sum(x => x.Quantity)}");

                return uniqueItems;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Récupère la quantité totale d'articles (en tenant compte des quantités)
        /// </summary>
        public async Task<int> GetCartTotalQuantityAsync()
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    return 0;
                }

                var items = await GetCartItemsAsync();
                return items.Sum(x => x.Quantity);
            }
            catch
            {
                return 0;
            }
        }

        private async Task SaveCartAsync()
        {
            try
            {
                if (!await IsJavaScriptAvailableAsync())
                {
                    Console.WriteLine("Impossible de sauvegarder - JavaScript non disponible");
                    return;
                }

                // Log détaillé avant sérialisation
                Console.WriteLine($"Sauvegarde de {_cartItems.Count} items:");
                for (int i = 0; i < _cartItems.Count; i++)
                {
                    var item = _cartItems[i];
                    Console.WriteLine($"  Item {i}: ProductId={item.ProductId}, Quantity={item.Quantity}");
                }

                var cartJson = JsonSerializer.Serialize(_cartItems, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"JSON à sauvegarder: {cartJson}");

                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", CART_KEY, cartJson);
                Console.WriteLine($"Panier sauvegardé avec succès");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde du panier: {ex.Message}");
                throw;
            }
        }
    }

    // Classe simplifiée pour le stockage localStorage
    public class CartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}