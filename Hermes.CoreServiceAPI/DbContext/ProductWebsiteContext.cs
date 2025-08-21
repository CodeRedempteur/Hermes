using Hermes.Data.Product.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermes.CoreServiceAPI
{
    public class ProductWebsiteContext : DbContext
    {
        public ProductWebsiteContext(DbContextOptions<ProductWebsiteContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Product> Products { get; set; }
        public DbSet<ImageProduct> Images { get; set; }
        public DbSet<Plastique> Plastiques { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Categorie> Categories { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Seo> Seos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuration explicite pour éviter les erreurs de mapping

            // Table Products
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("t_product");
                entity.HasKey(e => e.Id);

                // Propriétés
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Prix).HasColumnName("prix").IsRequired();
                entity.Property(e => e.IsPublished).HasColumnName("is_published").HasDefaultValue(false);
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Foreign keys
                entity.Property(e => e.ImageId).HasColumnName("image_id");
                entity.Property(e => e.PlastiqueId).HasColumnName("plastique_id");
                entity.Property(e => e.CategorieId).HasColumnName("categorie_id");
                entity.Property(e => e.TagId).HasColumnName("tag_id");
                entity.Property(e => e.StockId).HasColumnName("stock_id");
                entity.Property(e => e.SeoId).HasColumnName("seo_id");

                // Ignorer les propriétés de navigation pour éviter les erreurs automatiques
                entity.Ignore(e => e.Image);
                entity.Ignore(e => e.Plastique);
                entity.Ignore(e => e.Categorie);
                entity.Ignore(e => e.Tag);
                entity.Ignore(e => e.Stock);
                entity.Ignore(e => e.Seo);
            });

            // Table Images
            modelBuilder.Entity<ImageProduct>(entity =>
            {
                entity.ToTable("images");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.ImageBase64).HasColumnName("image_base64").HasColumnType("jsonb");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Table Plastiques
            modelBuilder.Entity<Plastique>(entity =>
            {
                entity.ToTable("plastiques");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
                entity.Property(e => e.CoutGramme).HasColumnName("cout_gramme").IsRequired();
            });

            // Table Tags
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("tags");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
            });

            // Table Categories
            modelBuilder.Entity<Categorie>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
            });

            // Table Stocks
            modelBuilder.Entity<Stock>(entity =>
            {
                entity.ToTable("stocks");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.TypeStock).HasColumnName("type_stock").IsRequired();
            });

            // Table Seos
            modelBuilder.Entity<Seo>(entity =>
            {
                entity.ToTable("seos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.WorkspaceId).HasColumnName("workspace_id").HasDefaultValue(0);
                entity.Property(e => e.Titre).HasColumnName("titre").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
            });
        }
    }
}