using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class OrderGroupItemVM
    {
        // Tên trạng thái nhóm đơn hàng (Chờ thanh toán, Đang xử lý, Đang vận chuyển, Đã giao, Đã hủy)
        public string StatusName { get; set; }

        // Tổng số đơn thuộc trạng thái này của khách hàng
        public int TotalCount { get; set; }

        // Có đang mở rộng tối đa 50 đơn hay không
        public bool IsExpanded { get; set; }

        // Danh sách đơn hàng của nhóm (mặc định 2 đơn, nếu mở rộng thì tối đa 50 đơn)
        public List<Order> Orders { get; set; }

        public OrderGroupItemVM()
        {
            Orders = new List<Order>();
        }
    }

    public class MyOrderVM
    {
        // Tiêu chí tìm kiếm theo Mã đơn hàng hoặc Tên sản phẩm
        public string SearchTerm { get; set; }

        // Tab trạng thái hiện tại (Tất cả đơn, Chờ thanh toán, Đang xử lý, Đang vận chuyển, Đã giao, Đã hủy)
        public string ActiveStatus { get; set; }

        // Trạng thái đang được bấm "Xem thêm"
        public string ExpandedStatus { get; set; }

        // Tổng số đơn hàng mà khách hàng này đã từng đặt trong hệ thống
        public int TotalUserOrders { get; set; }

        // Danh sách các nhóm trạng thái hiển thị
        public List<OrderGroupItemVM> OrderGroups { get; set; }

        public MyOrderVM()
        {
            ActiveStatus = "Tất cả đơn";
            OrderGroups = new List<OrderGroupItemVM>();
        }
    }
}
