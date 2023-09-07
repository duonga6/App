using System.Collections.Generic;
using App.Areas.Product.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace App.Areas.Product.Services
{
    public class CartService
    {
        public const string CARTKEY = "cart";

        private readonly IHttpContextAccessor _context;

        private readonly HttpContext httpContext;

        public CartService(IHttpContextAccessor context)
        {
            _context = context;
            httpContext = context.HttpContext;
        }

        public List<CartItem> GetCartItems()
        {
            var session = httpContext.Session;
            string cartjson = session.GetString(CARTKEY);
            if (cartjson != null)
                return JsonConvert.DeserializeObject<List<CartItem>>(cartjson);
            return new List<CartItem>();
        }

        public void ClearCart()
        {
            var session = httpContext.Session;
            session.Remove(CARTKEY);
        }

        public void SaveCartSession(List<CartItem> cl)
        {
            var session = httpContext.Session;
            var cartjson = JsonConvert.SerializeObject(cl);
            session.SetString(CARTKEY, cartjson);
        }
    }
}