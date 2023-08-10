using System.Net;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Services;
using System.Linq;

namespace App.Controllers
{
    public class FirstController : Controller
    {

        private readonly ILogger<FirstController> _logger;

        private readonly ProductService _productService;

        public FirstController(ILogger<FirstController> logger, ProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }
        public string Index() {

            // this.HttpContext
            // this.Request
            // this.Response
            // this.User
            // this.ModelState
            // this.ViewData
            // this.ViewBag
            // this.Url
            // this.TempData

            _logger.LogInformation("Index Action");

            return "Tôi là Index của First Controller";
        }

        public void Nothing()
        {
            _logger.LogInformation("Nothing Actions");
            Response.Headers.Add("hi","Xin chao cac ban");
        }

        public object Anything()
        {
            return Math.Sqrt(2);
        }

        public IActionResult Readme()
        {
            string content = @"
                Xin chào các bạn,
                các bạn đang học về ASP.NET MVC


                XUANTHULAB.NET
            ";

            return Content(content, "text/html");
        }

        public IActionResult Bird()
        {
            string path = Path.Combine(Startup.RootPath, "Files", "bird.jpg");
            var bytes = System.IO.File.ReadAllBytes(path);

            return File(bytes, "image/jpg");
        }

        public IActionResult IPhonePrice()
        {
            return Json( new {
                Name = "Iphone X",
                Price = 1000
            });
        }

        public IActionResult Privacy()
        {
            var url = Url.Action("Privacy", "Home");
            return LocalRedirect(url);
        }

        public IActionResult Google()
        {
            var url = "https://google.com.vn";
            return Redirect(url);
        }

        public IActionResult HelloView(string username)
        {
            if (string.IsNullOrEmpty(username))
                username = "Khách";
            // return View("/MyView/Xinchao.cshtml", username);
            return View("Xinchao2", username);
        }

        public IActionResult ViewProduct(int? id)
        {
            var product = _productService.Where(sp => sp.Id == id).FirstOrDefault();
            if (product == null) 
            {
                TempData["thongbao"] = "Sản phẩm bạn yêu cầu không có";
                return Redirect(Url.Action("Index", "Home"));
            }

            ViewData["product"] = product;
            ViewBag.Title = product.Name;
            return View();
        }
    }
}