using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Data.Product.Models
{
    // Classe pour gérer les images côté client
    public class ProductImage
    {
        public string DataUrl { get; set; } = string.Empty;
        public string Base64Data { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsPrimary { get; set; } = false;
        public bool IsUrl => Base64Data.StartsWith("http://") || Base64Data.StartsWith("https://");
    }

}
