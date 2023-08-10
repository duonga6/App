using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Models;

namespace App.Services
{
    public class ProductService : List<ProductModel>
    {
        public ProductService()
        {
            this.AddRange( new ProductModel[] {
                new ProductModel(){ Id = 1, Name = "SP1", Price=1000},
                new ProductModel(){ Id = 2, Name = "SP2", Price=1500},
                new ProductModel(){ Id = 3, Name = "SP3", Price=8000},
                new ProductModel(){ Id = 4, Name = "SP4", Price=6000},
            });
        }
    }
}