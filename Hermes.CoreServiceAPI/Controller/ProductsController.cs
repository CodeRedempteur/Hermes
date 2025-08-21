using Hermes.CoreServiceAPI;
using Hermes.Data.Product.Models;
using Hestia.CoreServicesAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hestia.CoreServicesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductWebsiteContext _context;

        public ProductsController(ProductWebsiteContext context)
        {
            _context = context;
        }

        // GET: api/Products - Version simple sans Include
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            // Version simple sans relations pour éviter les erreurs
            return await _context.Products.ToListAsync();
        }

        // GET: api/Products/5 - Version simple
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return product;
        }

        // GET: api/Products/5/details - Version avec relations manuelles
        [HttpGet("{id}/details")]
        public async Task<ActionResult<object>> GetByIdWithDetails(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Charger manuellement les relations si nécessaire
            ImageProduct? image = null;
            Categorie? categorie = null;
            Seo? seo = null;

            if (product.ImageId.HasValue)
            {
                image = await _context.Images
                    .FirstOrDefaultAsync(i => i.Id == product.ImageId.Value);
            }

            if (product.CategorieId.HasValue)
            {
                categorie = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == product.CategorieId.Value);
            }

            if (product.SeoId.HasValue)
            {
                seo = await _context.Seos
                    .FirstOrDefaultAsync(s => s.Id == product.SeoId.Value);
            }

            return new
            {
                Product = product,
                Image = image,
                Categorie = categorie,
                Seo = seo
            };
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            try
            {
                Console.WriteLine("=== CREATING PRODUCT ===");
                Console.WriteLine($"Product Name: {product.Nom}");
                Console.WriteLine($"Price: {product.Prix}");

                // Définir CreatedAt côté serveur
                product.CreatedAt = DateTime.UtcNow;
                product.Id = 0;

                // Nettoyer les propriétés de navigation pour éviter les erreurs
                product.Image = null;
                product.Categorie = null;
                product.Seo = null;
                product.Plastique = null;
                product.Tag = null;
                product.Stock = null;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Product created successfully with ID: {product.Id}");

                return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Create: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return BadRequest($"Error creating product: {ex.Message}");
            }
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            // Récupérer le produit existant
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            // Mettre à jour uniquement les propriétés nécessaires
            existingProduct.Nom = product.Nom;
            existingProduct.Description = product.Description;
            existingProduct.Prix = product.Prix;
            existingProduct.IsPublished = product.IsPublished;
            existingProduct.ImageId = product.ImageId;
            existingProduct.CategorieId = product.CategorieId;
            existingProduct.SeoId = product.SeoId;
            existingProduct.PlastiqueId = product.PlastiqueId;
            existingProduct.TagId = product.TagId;
            existingProduct.StockId = product.StockId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Products/5/publish
        [HttpPut("{id}/publish")]
        public async Task<IActionResult> Publish(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.IsPublished = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Products/5/unpublish
        [HttpPut("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.IsPublished = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}