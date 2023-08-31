using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace App.Models.Product
{
    [Table("Product")]
    public class ProductModel
    {
        [Key]
        public int ProductID { set; get; }

        [Required(ErrorMessage = "Phải có tiêu đề bài viết")]
        [Display(Name = "Tên sản phẩm")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        public string Title { set; get; }

        [Display(Name = "Mô tả ngắn")]
        public string Description { set; get; }

        [Display(Name = "Chuỗi định danh (url)", Prompt = "Nhập hoặc để trống tự phát sinh theo Title")]
        [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        public string Slug { set; get; }

        [Display(Name = "Nội dung")]
        public string Content { set; get; }

        [Display(Name = "Hiển thị")]
        public bool Published { set; get; }

        [Display(Name = "Giá")]
        [Range(0, int.MaxValue)]
        [Column(TypeName = "decimal")]
        public decimal Price {set;get;}

        public List<ProductCategory> ProductCategories { get; set; }

        [Display(Name = "Người đăng")]
        public string AuthorId { set; get; }

        
        [ForeignKey("AuthorId")]
        [Display(Name = "Người đăng")]
        public AppUser Author { set; get; }

        [Display(Name = "Ngày tạo")]
        public DateTime DateCreated { set; get; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime DateUpdated { set; get; }
    }
}