using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
    public class Contact 
    {
        [Key]
        public int Id {set;get;}

        [Display(Name = "Họ tên")]
        [Column(TypeName = "nvarchar")]
        [StringLength(30)]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string FullName {set;get;}

        [Display(Name = "Địa chỉ Email")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [EmailAddress(ErrorMessage = "{0} không đúng định dạng")]
        [StringLength(100)]
        public string Email {set;get;}

        [Display(Name = "Ngày gửi")]
        [DataType(DataType.Date)]
        public DateTime DateSent {set;get;}

        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "{0} không được để trống")]
        [Column(TypeName = "nvarchar")]
        [StringLength(100)]
        public string Message {set;get;}

        [Display(Name = "Số điện thoại")]
        [StringLength(15)]
        [Phone(ErrorMessage = "{0} không đúng định dạng")]
        public string Phone {set;get;}
    }
}