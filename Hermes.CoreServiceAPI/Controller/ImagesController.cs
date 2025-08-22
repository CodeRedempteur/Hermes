using Hermes.CoreServiceAPI;
using Hermes.Data.Product.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hestia.CoreServicesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly ProductWebsiteContext _context;

        public ImagesController(ProductWebsiteContext context)
        {
            _context = context;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImageProduct>>> GetAll()
        {
            return await _context.Images.ToListAsync();
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageProduct>> GetById(int id)
        {
            var image = await _context.Images.FindAsync(id);

            if (image == null)
                return NotFound();

            return image;
        }

        // GET: api/Images/product/5
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ImageProduct>>> GetByProductId(int productId)
        {
            var images = await _context.Images
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.Id)
                .ToListAsync();

            return images;
        }

        // POST: api/Images
        [HttpPost]
        public async Task<ActionResult<ImageProduct>> Create(ImageProduct image)
        {
            try
            {
                Console.WriteLine("=== CREATING IMAGE ===");
                Console.WriteLine($"Product ID: {image.ProductId}");
                Console.WriteLine($"Base64/URL Length: {image.ImageBase64?.Length ?? 0}");
                Console.WriteLine($"Is URL: {(image.ImageBase64?.StartsWith("http") == true)}");

                // Définir CreatedAt côté serveur
                image.CreatedAt = DateTime.UtcNow;
                image.Id = 0;

                // Nettoyer la propriété de navigation
                image.Product = null;

                _context.Images.Add(image);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Image created successfully with ID: {image.Id}");

                return CreatedAtAction(nameof(GetById), new { id = image.Id }, image);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Create Image: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return BadRequest($"Error creating image: {ex.Message}");
            }
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _context.Images.FindAsync(id);

            if (image == null)
                return NotFound();

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Images/5/product/10
        [HttpPut("{imageId}/product/{productId}")]
        public async Task<IActionResult> AssignToProduct(int imageId, int productId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
                return NotFound("Image not found");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound("Product not found");

            image.ProductId = productId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Images/5/data - Récupère l'image avec info sur le type
        [HttpGet("{id}/data")]
        public async Task<ActionResult<object>> GetImageData(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            var isUrl = image.ImageBase64?.StartsWith("http") == true;

            return new
            {
                Id = image.Id,
                ImageData = image.ImageBase64,
                IsUrl = isUrl,
                IsBase64 = !isUrl,
                ProductId = image.ProductId,
                CreatedAt = image.CreatedAt
            };
        }
    }
}