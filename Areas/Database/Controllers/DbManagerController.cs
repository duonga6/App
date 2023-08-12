using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
    [Area("Database")]
    [Route("/database-manage/[action]")]
    public class DbManagerController : Controller
    {

        private readonly AppDbContext _dbContext;

        public DbManagerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
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
    }
}