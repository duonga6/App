using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.Contacts;
using App.Models.Blog;
using App.Models.Product;

namespace App.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            modelBuilder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug)
                        .IsUnique();
            });

            modelBuilder.Entity<PostCategory>(entity => {
                entity.HasKey(c => new {
                    c.PostID,
                    c.CategoryID
                });
            });

            modelBuilder.Entity<Post>(entity => {
                entity.HasIndex(p => p.Slug)
                        .IsUnique();
            });

            modelBuilder.Entity<CategoryProduct>(entity => {
                entity.HasIndex(c => c.Slug)
                        .IsUnique();
            });

            modelBuilder.Entity<ProductCategory>(entity => {
                entity.HasKey(c => new {
                    c.ProductID,
                    c.CategoryID
                });
            });

            modelBuilder.Entity<ProductModel>(entity => {
                entity.HasIndex(p => p.Slug)
                        .IsUnique();
            });
        }

        public DbSet<Contact> Contacts {set;get;}

        public DbSet<Category> Categories {set;get;}

        public DbSet<Post> Posts {set;get;}

        public DbSet<PostCategory> PostCategories {set;get;}

        public DbSet<CategoryProduct> CategoryProducts {set;get;}

        public DbSet<ProductModel> Products {set;get;}

        public DbSet<ProductCategory> ProductCategories {set;get;}

        public DbSet<ProductPhoto> ProductPhotos {set;get;}

    }
}
