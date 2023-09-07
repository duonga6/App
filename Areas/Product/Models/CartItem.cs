using System.ComponentModel.DataAnnotations;
using App.Models.Product;

namespace App.Areas.Product.Models
{
    public class CartItem
    {
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity {set;get;}
        public ProductModel Product {set;get;}
    }
}