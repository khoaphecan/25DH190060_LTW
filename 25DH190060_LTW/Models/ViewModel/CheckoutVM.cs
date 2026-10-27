using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class CheckoutVM
    {
        /* Lưu thông tin form Checkout */
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        public int CustomerID { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Display(Name = "Tổng giá trị")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức giao hàng")]
        [Display(Name = "Phương thức giao hàng")]
        public string ShippingMethod { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        public string Username { get; set; }

        // Các thuộc tính khác của đơn hàng
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
