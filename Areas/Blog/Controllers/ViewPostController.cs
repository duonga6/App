using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        
        private readonly ILogger<ViewPostController> _logger;

        private readonly AppDbContext _context;

        private readonly int PAGE_SIZE = 10;

        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        [Route("/post/{slug?}", Name = "ViewPostPage")]
        public IActionResult Index(string slug,[FromQuery(Name = "p")] int currentPage)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.slug = slug;

            Category category = null;
            if (!string.IsNullOrEmpty(slug))
            {
                category = _context.Categories
                .Include(c => c.CategoryChildren)
                .Where(c => c.Slug == slug)
                .FirstOrDefault();



                if (category == null) return NotFound();
                
            }

            var post = _context.Posts
            .Include(c => c.Author)
            .Include(c => c.PostCategories)
            .ThenInclude(c => c.Category)
            .AsQueryable();

            post.OrderByDescending(p => p.DateCreated);

            

            if (category != null)
            {
                // Lấy Id các Cate con của category để lọc post theo danh mục
                List<int> ids = new List<int>();
                category.ChildCategoriesID(null, ids);
                ids.Add(category.Id);

                post = post.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
            }

            ViewBag.category = category;

            int totalPosts = post.Count();
            int pageCount = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > pageCount) currentPage = pageCount;

            ViewBag.CurrentPage = currentPage;
            ViewBag.CountPage = pageCount;

            ViewBag.totalPosts = totalPosts;

            var postInPage = post.Count() > 0 ? post.OrderByDescending(p => p.DateCreated)
                                .Skip((currentPage - 1) * PAGE_SIZE)
                                .Take(PAGE_SIZE) : post;

            return View(postInPage.ToList());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Details(string postslug)
        {
            var post = _context.Posts.Where(p => p.Slug == postslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault();
            
            if (post == null) return NotFound("Không tìm thấy bài viết");

            var categories = GetCategories();
            ViewBag.categories = categories;

            Category category = post.PostCategories.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPost = _context.Posts.Where(p => p.PostCategories.Any(pc => pc.Category.Id == category.Id))
                                    .Where(p => p.PostId != post.PostId)
                                    .OrderByDescending(p => p.DateCreated)
                                    .Take(5)
                                    .ToList();
            
            ViewBag.otherPost = otherPost;

            return View(post);
        }

        private List<Category> GetCategories()
        {
            var qr = _context.Categories
                    .Include(c => c.CategoryChildren)
                    .AsEnumerable()
                    .Where(c => c.ParentCategory == null)
                    .ToList();
            return qr;
        }
    }
}