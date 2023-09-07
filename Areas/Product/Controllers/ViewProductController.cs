using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using App.Models.Product;
using App.Areas.Product.Services;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        
        private readonly ILogger<ViewProductController> _logger;

        private readonly AppDbContext _context;

        private readonly CartService _cartService;

        private readonly int PAGE_SIZE = 10;

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }
        
        [Route("/product/{slug?}")]
        public IActionResult Index(string slug,[FromQuery(Name = "p")] int currentPage)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.slug = slug;

            CategoryProduct category = null;
            if (!string.IsNullOrEmpty(slug))
            {
                category = _context.CategoryProducts
                .Include(c => c.CategoryChildren)
                .Where(c => c.Slug == slug)
                .FirstOrDefault();



                if (category == null) return NotFound();
                
            }

            var product = _context.Products
            .Include(c => c.Photos)
            .Include(c => c.Author)
            .Include(c => c.ProductCategories)
            .ThenInclude(c => c.Category)
            .AsSplitQuery();

            product.OrderByDescending(p => p.DateCreated);

            

            if (category != null)
            {
                // Lấy Id các Cate con của category để lọc post theo danh mục
                List<int> ids = new();
                category.ChildCategoriesID(null, ids);
                ids.Add(category.Id);

                product = product.Where(p => p.ProductCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }

            ViewBag.category = category;

            int totalProducts = product.Count();
            int pageCount = (int)Math.Ceiling((double)totalProducts / PAGE_SIZE);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > pageCount) currentPage = pageCount;

            ViewBag.CurrentPage = currentPage;
            ViewBag.CountPage = pageCount;

            ViewBag.totalPosts = totalProducts;

            var productInPage = product.Any() ? product.OrderByDescending(p => p.DateCreated)
                                .Skip((currentPage - 1) * PAGE_SIZE)
                                .Take(PAGE_SIZE) : product;

            return View(productInPage.ToList());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Details(string productslug)
        {
            var product = _context.Products.Where(p => p.Slug == productslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .AsSplitQuery()
                                    .FirstOrDefault();
            
            if (product == null) return NotFound("Không tìm thấy bài viết");

            var categories = GetCategories();
            ViewBag.categories = categories;

            CategoryProduct category = product.ProductCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherProduct = _context.Products.Where(p => p.ProductCategories.Any(pc => pc.Category.Id == category.Id))
                                    .Where(p => p.ProductID != product.ProductID)
                                    .OrderByDescending(p => p.DateCreated)
                                    .Take(5)
                                    .ToList();
            
            ViewBag.otherProduct = otherProduct;

            return View(product);
        }

        private List<CategoryProduct> GetCategories()
        {
            var qr = _context.CategoryProducts
                    .Include(c => c.CategoryChildren)
                    .AsEnumerable()
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            return qr;
        }

        [Route("/cart")]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }

        [Route("/addcart/{productid:int}", Name = "AddCart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {
            var product = _context.Products.Find(productid);
            if (product == null) return NotFound();

            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductID == productid);
            if (cartitem == null)
                cart.Add(new Models.CartItem {
                    Quantity = 1,
                    Product = product
                });
            else
            {
                cartitem.Quantity++;
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [Route("/deletecartitem/{productid:int}")]
        public IActionResult DeleteCartItem([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(c => c.Product.ProductID == productid);
            if (cartitem != null)
                cart.Remove(cartitem);
            _cartService.SaveCartSession(cart);

            return RedirectToAction(nameof(Cart));
        }

        [Route("/updatequantitycart")]
        [HttpPost]
        public IActionResult UpdateQuantityCart(int productid, int quantity)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(c => c.Product.ProductID == productid);
            if (cartitem == null) return NotFound();

            cartitem.Quantity = quantity;
            _cartService.SaveCartSession(cart);

            return Ok();
        }
    }
}