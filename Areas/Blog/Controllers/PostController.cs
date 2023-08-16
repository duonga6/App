using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("/admin/blod/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        private const int POST_PER_PAGE = 10;

        [TempData]
        public string StatusMessage { set; get; }

        public PostController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Blog/Post
        [Route("/admin/blod/post/index", Name = "BlogPostIndexRoute")]
        public async Task<IActionResult> Index([FromQuery(Name = "page")] int currentPage)
        {
            var posts = _context.Posts.Include(p => p.Author).OrderByDescending(p => p.DateUpdated);

            int totalPosts = _context.Posts.Count();
            int pageCount = (int)Math.Ceiling((double)totalPosts / POST_PER_PAGE);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > pageCount) currentPage = pageCount;

            var pagingModel = new PagingModel()
            {
                countpages = pageCount,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.RouteUrl("BlogPostIndexRoute", new { page = pageNumber })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            var postInPage = await posts.Skip((currentPage - 1) * POST_PER_PAGE)
                                .Take(POST_PER_PAGE)
                                .Include(p => p.PostCategories)
                                .ThenInclude(p => p.Category)
                                .ToListAsync();

            return View(postInPage);
        }

        // GET: Blog/Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var cate = await _context.Categories.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published")] CreatePostModel post)
        {
            
            var cate = await _context.Categories.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (_context.Posts.Any(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError(string.Empty, "Slug này đã được dùng, hãy nhập slug khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                post.AuthorId = user.Id;
                post.DateCreated = post.DateUpdated = DateTime.Now;

                if (post.CategoriesID != null)
                {
                    foreach (var cateid in post.CategoriesID)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryID = cateid,
                            Post = post
                        });
                    }
                }

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var createPost = new CreatePostModel() {
                PostId = post.PostId,
                Title = post.Title,
                Content = post.Content,
                Published = post.Published,
                Slug = post.Slug,
                Description = post.Description,
                CategoriesID =  post.PostCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var cate = await _context.Categories.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            return View(createPost);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published, CategoriesID")] CreatePostModel post)
        {
            var cate = await _context.Categories.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");
             
            if (id != post.PostId)
            {
                return NotFound($"Không tìm thấy ID: id - {id} postid - {post.PostId}");
            }

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }

            if (_context.Posts.Any(p => p.Slug == post.Slug && p.PostId != post.PostId))
            {
                ModelState.AddModelError(string.Empty, "Slug này đã được sử dụng, hãy chọn slug khác");
                return View(post);
            }

            if (ModelState.IsValid)
            {

                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null) return NotFound();


                    postUpdate.Title = post.Title;
                    postUpdate.Description = post.Description;
                    postUpdate.Slug = post.Slug;
                    postUpdate.Content = post.Content;
                    postUpdate.Published = post.Published;
                    postUpdate.DateUpdated = DateTime.Now;

                    if (post.CategoriesID == null) post.CategoriesID = new int[]{};

                    var oldCateId = postUpdate.PostCategories.Select(p => p.CategoryID);
                    var newCateID = post.CategoriesID;

                    var removeCate = from postCate in _context.PostCategories
                                        where (!newCateID.Contains(postCate.CategoryID))
                                        select postCate;

                    _context.PostCategories.RemoveRange(removeCate);

                    var addCate = newCateID.Where(c => !oldCateId.Contains(c));
                    foreach(var item in addCate)
                    {
                        _context.PostCategories.Add(new PostCategory() {
                            CategoryID = item,
                            PostID = id
                        });
                    }


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công bài viết: " + post.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
