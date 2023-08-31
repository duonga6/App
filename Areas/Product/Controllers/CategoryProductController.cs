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

namespace App.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("/admin/categoryproduct/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryProductController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Blog/Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var cate = (await qr.ToListAsync())
                        .Where(c => c.ParentCategory == null)
                        .ToList();

            return View(cate);
        }

        // GET: Blog/Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        private void CreateItem(List<CategoryProduct> src, List<CategoryProduct> des, int level)
        {
            foreach (var category in src)
            {
                string prefix = string.Concat(Enumerable.Repeat("----", level));
                des.Add(new CategoryProduct()
                {
                    Id = category.Id,
                    Title = prefix + category.Title
                });
                if (category.CategoryChildren?.Count > 0)
                {
                    CreateItem(category.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }

        // GET: Blog/Category/Create
        public async Task<IActionResult> CreateAsync()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var cate = (await qr.ToListAsync())
                        .Where(c => c.ParentCategory == null)
                        .ToList();

            cate.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            var items = new List<CategoryProduct>();

            CreateItem(cate, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;


            return View();
        }

        // POST: Blog/Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var cate = (await qr.ToListAsync())
                        .Where(c => c.ParentCategory == null)
                        .ToList();

            cate.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            var items = new List<CategoryProduct>();

            CreateItem(cate, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;

            return View(category);
        }

        // GET: Blog/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var cate = (await qr.ToListAsync())
                        .Where(c => c.ParentCategory == null)
                        .ToList();

            cate.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            var items = new List<CategoryProduct>();

            CreateItem(cate, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;

            return View(category);
        }

        // POST: Blog/Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (category.Id == category.ParentCategoryId)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác với danh mục này");
                canUpdate = false;
            }

            if (canUpdate && category.ParentCategoryId != null)
            {
                var childCates = (from c in _context.CategoryProducts select c)
                                    .AsNoTracking()
                                    .Include(c => c.CategoryChildren)
                                    .ToList()
                                    .Where(c => c.ParentCategoryId == category.Id);

                Func<List<CategoryProduct>, bool> checkCateIds = null;
                checkCateIds = (cates) =>
                {
                    foreach (var cate in cates)
                    {
                        if (cate.Id == category.ParentCategoryId)
                        {
                            canUpdate = false;
                            ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                            return true;
                        }
                        if (cate.CategoryChildren != null)
                            return checkCateIds(cate.CategoryChildren.ToList());
                    }
                    return false;
                };

                checkCateIds(childCates.ToList());
            }

            if (ModelState.IsValid && canUpdate)
            {
                Console.WriteLine("ABC");
                try
                {
                    if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.CategoryChildren);

            var cate = (await qr.ToListAsync())
                        .Where(c => c.ParentCategory == null)
                        .ToList();

            cate.Insert(0, new CategoryProduct()
            {
                Title = "Không có danh mục cha",
                Id = -1
            });

            var items = new List<CategoryProduct>();

            CreateItem(cate, items, 0);

            var selectList = new SelectList(items, "Id", "Title");

            ViewData["ParentCategoryId"] = selectList;

            return View(category);
        }

        // GET: Blog/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Blog/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryProducts
                                    .Include(c => c.CategoryChildren)
                                    .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            foreach (var item in category.CategoryChildren)
            {
                item.ParentCategoryId = category.ParentCategoryId;
            }


            _context.CategoryProducts.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
