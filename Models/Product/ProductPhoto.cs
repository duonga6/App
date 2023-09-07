using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models.Product
{
    [Table("ProductPhoto")]
    public class ProductPhoto
    {
        public int Id {set;get;}

        public string FileName {set;get;}

        public int ProductID {set;get;}

        [ForeignKey("ProductID")]
        public ProductModel Product {set;get;}
    }
}