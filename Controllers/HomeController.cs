using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Models;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public string HiHome() => "Xin chào các bạn, tôi là HiHome";

        public IActionResult Index()
        {
            var products = _context.Products
            .Include(c => c.Photos)
            .Include(c => c.Author)
            .Include(c => c.ProductCategories)
            .ThenInclude(c => c.Category)
            .AsSplitQuery();

            products = products.OrderByDescending(p => p.DateCreated).Take(4);

            var posts = _context.Posts
            .Include(c => c.Author)
            .Include(c => c.PostCategories)
            .ThenInclude(c => c.Category)
            .AsQueryable();

            posts = posts.OrderByDescending(p => p.DateCreated).Take(4);

            ViewBag.posts = posts.ToList();
            ViewBag.products = products.ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
