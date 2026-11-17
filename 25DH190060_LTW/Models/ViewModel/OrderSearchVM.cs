using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class OrderSearchVM
    {
        // Tiêu chí tìm kiếm theo tên khách hàng
        public string CustomerName { get; set; }

        // Tiêu chí lọc theo trạng thái thanh toán
        public string PaymentStatus { get; set; }

        // Tiêu chí lọc theo ngày đặt hàng
        public DateTime? OrderDate { get; set; }

        // Phân trang
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;

        // Danh sách đơn hàng phân trang
        public IPagedList<Order> Orders { get; set; }

        // Danh sách trạng thái thanh toán phục vụ DropdownList
        public List<SelectListItem> PaymentStatusList { get; set; }

        public OrderSearchVM()
        {
            PageSize = 15;
            PaymentStatusList = new List<SelectListItem>
            {
                new SelectListItem { Text = "-- Tất cả trạng thái --", Value = "" },
                new SelectListItem { Text = "Chưa thanh toán", Value = "Chưa thanh toán" },
                new SelectListItem { Text = "Thanh toán tiền mặt", Value = "Thanh toán tiền mặt" },
                new SelectListItem { Text = "Thanh toán paypal", Value = "Thanh toán paypal" },
                new SelectListItem { Text = "Trả góp", Value = "Trả góp" },
                new SelectListItem { Text = "Đã thanh toán", Value = "Đã thanh toán" },
                new SelectListItem { Text = "Đã hủy", Value = "Đã hủy" }
            };
        }
    }
}
