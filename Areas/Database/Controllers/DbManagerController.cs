using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
using App.Models.Blog;
using App.Models.Product;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManagerController : Controller
    {

        private readonly AppDbContext _dbContext;

        private readonly UserManager<AppUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        public DbManagerController(AppDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DeleteDB()
        {
            return View();
        }

        [TempData]
        public string StatusMessage {set;get;}

        [HttpPost]
        public async Task<IActionResult> DeleteDBAsync()
        {
            var succes = await _dbContext.Database.EnsureDeletedAsync();

            StatusMessage = succes ? "Xóa thành công" : "Xóa thất bại";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Migrations()
        {
           await _dbContext.Database.MigrateAsync();
           StatusMessage = "Cập nhật thành công";
           return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> SeedDataAsync()
        {
            var rolenames = typeof(RoleName).GetFields().ToList();
            foreach (var r in rolenames)
            {
                var rolename = (string)r.GetRawConstantValue();
                var rfound = await _roleManager.FindByNameAsync(rolename);
                if (rfound == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(rolename));
                }
            }

            var useradmin = await _userManager.FindByNameAsync("admin");
            if (useradmin == null)
            {
                var uadmin = new AppUser()
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(uadmin, "admin123");
                await _userManager.AddToRoleAsync(uadmin, RoleName.Administrator);
            }

            SeedPostCategory();
            SeedProductCategory();

            StatusMessage = "Vừa Seed Database";
            return RedirectToAction("Index");
        }

        private void SeedPostCategory()
        {

            _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Content.Contains("[FakeData]")));
            _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(c => c.Content.Contains("[FakeData]")));

            _dbContext.SaveChanges();
            
            // Phát sinh Category
            var fakerCategory = new Faker<Category>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM {cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentence(5) + "[FakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;

            cate211.ParentCategory = cate21;
            cate21.ParentCategory = cate2;

            var categories = new Category[] {cate1, cate11, cate12, cate2, cate21, cate211};
            _dbContext.Categories.AddRange(categories);

            // Phát sinh Post
            var randomCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerPost = new Faker<Post>();
            fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
            fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[FakeData]");
            fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2023,8,12)));
            fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerPost.RuleFor(p => p.Published, f => true);
            fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

            List<Post> posts = new List<Post>();
            List<PostCategory> postCategories = new List<PostCategory>();

            for (int i = 1; i <= 40; i++)
            {
                var post = fakerPost.Generate();
                post.DateUpdated = post.DateCreated;
                posts.Add(post);
                postCategories.Add(new PostCategory() {
                    Post = post,
                    Category = categories[randomCateIndex.Next(5)]
                });
            }

            _dbContext.Posts.AddRange(posts);
            _dbContext.PostCategories.AddRange(postCategories);

            _dbContext.SaveChanges();
        }

        private void SeedProductCategory()
        {

            _dbContext.CategoryProducts.RemoveRange(_dbContext.CategoryProducts.Where(c => c.Content.Contains("[FakeData]")));
            _dbContext.Products.RemoveRange(_dbContext.Products.Where(c => c.Content.Contains("[FakeData]")));

            _dbContext.SaveChanges();

            // Phát sinh Category
            var fakerCategory = new Faker<CategoryProduct>();
            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"Nhóm SP {cm++} " + fk.Lorem.Sentence(1,2).Trim('.'));
            fakerCategory.RuleFor(c => c.Content, fk => fk.Lorem.Sentence(5) + "[FakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate1 = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate1;
            cate12.ParentCategory = cate1;

            cate211.ParentCategory = cate21;
            cate21.ParentCategory = cate2;

            var categories = new CategoryProduct[] {cate1, cate11, cate12, cate2, cate21, cate211};
            _dbContext.CategoryProducts.AddRange(categories);

            // Phát sinh Post
            var randomCateIndex = new Random();
            int bv = 1;

            var user = _userManager.GetUserAsync(this.User).Result;

            var fakerProduct = new Faker<ProductModel>();
            fakerProduct.RuleFor(p => p.AuthorId, f => user.Id);
            fakerProduct.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[FakeData]");
            fakerProduct.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021,1,1), new DateTime(2023,8,12)));
            fakerProduct.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
            fakerProduct.RuleFor(p => p.Published, f => true);
            fakerProduct.RuleFor(p => p.Slug, f => f.Lorem.Slug());
            fakerProduct.RuleFor(p => p.Title, f => $"Sản phẩm {bv++} " + f.Commerce.ProductName());
            fakerProduct.RuleFor(p => p.Price, f => int.Parse(f.Commerce.Price(500, 1000, 0)));

            List<ProductModel> products = new List<ProductModel>();
            List<ProductCategory> productCategories = new List<ProductCategory>();

            for (int i = 1; i <= 40; i++)
            {
                var product = fakerProduct.Generate();
                product.DateUpdated = product.DateCreated;
                products.Add(product);
                productCategories.Add(new ProductCategory() {
                    Product = product,
                    Category = categories[randomCateIndex.Next(5)]
                });
            }

            _dbContext.Products.AddRange(products);
            _dbContext.ProductCategories.AddRange(productCategories);

            _dbContext.SaveChanges();
        }

        
    }
}