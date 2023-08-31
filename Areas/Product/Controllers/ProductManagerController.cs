using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using App.Areas.Product.Models;

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("/admin/productmanager/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
    public class ProductManagerController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        private const int POST_PER_PAGE = 10;

        [TempData]
        public string StatusMessage { set; get; }

        public ProductManagerController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Blog/Post
        [Route("/admin/productmanager", Name = "ProductIndexRoute")]
        public async Task<IActionResult> Index([FromQuery(Name = "page")] int currentPage)
        {
            var products = _context.Products.Include(p => p.Author).OrderByDescending(p => p.DateUpdated);

            int totalPosts = _context.Products.Count();
            int pageCount = (int)Math.Ceiling((double)totalPosts / POST_PER_PAGE);
            if (currentPage < 1) currentPage = 1;
            if (currentPage > pageCount) currentPage = pageCount;

            var pagingModel = new PagingModel()
            {
                countpages = pageCount,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.RouteUrl("ProductIndexRoute", new { page = pageNumber })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            var postInPage = await products.Skip((currentPage - 1) * POST_PER_PAGE)
                                .Take(POST_PER_PAGE)
                                .Include(p => p.ProductCategories)
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

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Blog/Post/Create
        public async Task<IActionResult> CreateAsync()
        {
            var cate = await _context.CategoryProducts.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            return View();
        }

        // POST: Blog/Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published")] CreateProductModel product)
        {
            
            var cate = await _context.CategoryProducts.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            product.Slug ??= AppUtilities.GenerateSlug(product.Title);

            if (_context.Products.Any(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError(string.Empty, "Slug này đã được dùng, hãy nhập slug khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                product.AuthorId = user.Id;
                product.DateCreated = product.DateUpdated = DateTime.Now;

                if (product.CategoriesID != null)
                {
                    foreach (var cateid in product.CategoriesID)
                    {
                        _context.ProductCategories.Add(new ProductCategory()
                        {
                            CategoryID = cateid,
                            Product = product
                        });
                    }
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        // GET: Blog/Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.Include(p => p.ProductCategories).FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            var createProduct = new CreateProductModel() {
                ProductID = product.ProductID,
                Title = product.Title,
                Content = product.Content,
                Published = product.Published,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                CategoriesID =  product.ProductCategories.Select(pc => pc.CategoryID).ToArray()
            };

            var cate = await _context.CategoryProducts.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");

            return View(createProduct);
        }

        // POST: Blog/Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Title,Description,Slug,Content,Published, CategoriesID, Price")] CreateProductModel product)
        {
            var cate = await _context.CategoryProducts.ToListAsync();
            ViewBag.CateList = new MultiSelectList(cate, "Id", "Title");
             
            if (id != product.ProductID)
            {
                return NotFound($"Không tìm thấy ID: id - {id} postid - {product.ProductID}");
            }

            if (product.Slug == null)
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
            }

            if (_context.Products.Any(p => p.Slug == product.Slug && p.ProductID != product.ProductID))
            {
                ModelState.AddModelError(string.Empty, "Slug này đã được sử dụng, hãy chọn slug khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {

                try
                {
                    var productUpdate = await _context.Products.Include(p => p.ProductCategories).FirstOrDefaultAsync(p => p.ProductID == id);
                    if (productUpdate == null) return NotFound();


                    productUpdate.Title = product.Title;
                    productUpdate.Description = product.Description;
                    productUpdate.Slug = product.Slug;
                    productUpdate.Content = product.Content;
                    productUpdate.Published = product.Published;
                    productUpdate.DateUpdated = DateTime.Now;
                    productUpdate.Price = product.Price;

                    product.CategoriesID ??= new int[]{};

                    var oldCatePost = productUpdate.ProductCategories.Select(p => p);
                    var newCateID = product.CategoriesID;

                    var removeCate = oldCatePost.Where(c => !newCateID.Contains(c.CategoryID));

                    _context.ProductCategories.RemoveRange(removeCate);
                    

                    var addCate = newCateID.Where(c => !productUpdate.ProductCategories.Where(p => p.CategoryID == c).Any());
                    foreach(var item in addCate)
                    {
                        _context.ProductCategories.Add(new ProductCategory() {
                            CategoryID = item,
                            ProductID = id
                        });
                    }


                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(product.ProductID))
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
            return View(product);
        }

        // GET: Blog/Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Blog/Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            StatusMessage = "Xóa thành công bài viết: " + product.Title;
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Products.Any(e => e.ProductID == id);
        }
    }
}
