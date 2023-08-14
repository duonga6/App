using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Models;
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

            StatusMessage = "Vừa Seed Database";

            return RedirectToAction("Index");
        }
    }
}