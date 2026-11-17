using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;

namespace _25DH190060_LTW.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Order
        public ActionResult Index()
        {
            return RedirectToAction("MyOrder");
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            // Kiểm tra giỏ hàng trong session
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            // Xác thực người dùng đã đăng nhập chưa
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : (Session["Username"] != null ? Session["Username"].ToString() : "");
            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin khách hàng từ CSDL
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra số lượng tồn kho trước khi hiển thị Checkout
            foreach (var item in cart.Items)
            {
                var pro = db.Products.Find(item.ProductID);
                if (pro != null && pro.Quantity < item.Quantity)
                {
                    ViewBag.StockWarning = $"Sản phẩm '{pro.ProductName}' chỉ còn {pro.Quantity} trong kho. Vui lòng điều chỉnh lại số lượng đặt.";
                }
            }

            // Tạo dữ liệu hiển thị cho CheckoutVM
            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                OrderDate = DateTime.Now,
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                PaymentMethod = "Tiền mặt",
                ShippingMethod = "Giao hàng nhanh"
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            string username = User.Identity.IsAuthenticated ? User.Identity.Name : (Session["Username"] != null ? Session["Username"].ToString() : "");
            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Kiểm tra số lượng tồn kho: Nếu số lượng đặt > SL tồn thì báo lỗi không cho đặt
            foreach (var item in cart.Items)
            {
                var pro = db.Products.Find(item.ProductID);
                if (pro == null)
                {
                    ModelState.AddModelError("", $"Sản phẩm mã #{item.ProductID} không tồn tại.");
                }
                else if (pro.Quantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Sản phẩm '{pro.ProductName}' hiện chỉ còn {pro.Quantity} sản phẩm trong kho (bạn đặt {item.Quantity} sản phẩm). Vui lòng cập nhật lại giỏ hàng.");
                }
            }

            if (ModelState.IsValid)
            {
                // Thiết lập PaymentStatus và OrderStatus dựa trên PaymentMethod
                string paymentStatus = "Chưa thanh toán";
                string orderStatus = "Đang xử lý";

                switch (model.PaymentMethod)
                {
                    case "Tiền mặt":
                        paymentStatus = "Thanh toán tiền mặt";
                        orderStatus = "Đang xử lý";
                        break;
                    case "Paypal":
                        paymentStatus = "Thanh toán paypal";
                        orderStatus = "Đang xử lý";
                        break;
                    case "Mua trước trả sau":
                        paymentStatus = "Trả góp";
                        orderStatus = "Đang xử lý";
                        break;
                    default:
                        paymentStatus = "Chưa thanh toán";
                        orderStatus = "Chờ thanh toán";
                        break;
                }

                string deliveryAddress = !string.IsNullOrWhiteSpace(model.ShippingAddress)
                    ? model.ShippingAddress.Trim()
                    : (!string.IsNullOrWhiteSpace(customer.CustomerAddress) ? customer.CustomerAddress.Trim() : "Chưa cập nhật địa chỉ");

                // Tạo đối tượng Order và lưu vào DB
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalValue(),
                    PaymentStatus = paymentStatus,
                    OrderStatus = orderStatus,
                    PaymentMethod = !string.IsNullOrWhiteSpace(model.PaymentMethod) ? model.PaymentMethod : "Tiền mặt",
                    DeliveryMethod = !string.IsNullOrWhiteSpace(model.ShippingMethod) ? model.ShippingMethod : "Giao hàng nhanh",
                    AddressDelivery = deliveryAddress
                };
                db.Orders.Add(order);
                db.SaveChanges();

                // Tạo các chi tiết đơn hàng (OrderDetail) và TRỪ SỐ LƯỢNG TỒN KHO
                foreach (var item in cart.Items)
                {
                    var pro = db.Products.Find(item.ProductID);
                    if (pro != null)
                    {
                        pro.Quantity -= item.Quantity;
                        if (pro.Quantity < 0) pro.Quantity = 0;
                    }

                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    };
                    db.OrderDetails.Add(detail);
                }
                db.SaveChanges();

                // Xóa giỏ hàng sau khi đặt hàng thành công
                Session["Cart"] = null;

                // Điều hướng tới trang Xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            // Nếu model không hợp lệ (hết hàng hoặc lỗi validate), nạp lại dữ liệu để hiển thị
            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalValue();
            return View(model);
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetail.Select(d => d.Product))
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Order/MyOrder
        [Authorize]
        public ActionResult MyOrder(string searchTerm, string status, string expandStatus)
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : (Session["Username"] != null ? Session["Username"].ToString() : "");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy toàn bộ danh sách đơn hàng của khách hàng hiện tại
            var allUserOrders = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetail.Select(d => d.Product.Category))
                .Where(o => o.CustomerID == customer.CustomerID)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderID)
                .ToList();

            int totalUserOrders = allUserOrders.Count;
            string activeStatus = string.IsNullOrWhiteSpace(status) ? "Tất cả đơn" : status.Trim();

            var vm = new MyOrderVM
            {
                SearchTerm = searchTerm,
                ActiveStatus = activeStatus,
                ExpandedStatus = expandStatus,
                TotalUserOrders = totalUserOrders
            };

            // Trường hợp khách hàng chưa từng đặt đơn nào trong hệ thống
            if (totalUserOrders == 0)
            {
                return View(vm);
            }

            // Lọc theo từ khóa tìm kiếm (Mã đơn hàng hoặc Tên sản phẩm)
            var filteredOrders = allUserOrders.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                filteredOrders = filteredOrders.Where(o =>
                    o.OrderID.ToString().Contains(term) ||
                    (o.OrderDetail != null && o.OrderDetail.Any(od => od.Product != null && od.Product.ProductName.ToLower().Contains(term)))
                );
            }

            var standardStatuses = new List<string> { "Chờ thanh toán", "Đang xử lý", "Đang vận chuyển", "Đã giao", "Đã hủy" };

            if (activeStatus == "Tất cả đơn" || activeStatus == "Tất cả")
            {
                // Tab Tất cả đơn: Gom nhóm theo từng trạng thái đơn hàng
                foreach (var st in standardStatuses)
                {
                    var ordersInSt = filteredOrders.Where(o => o.OrderStatusDisplay == st).ToList();
                    if (ordersInSt.Any())
                    {
                        bool isExpanded = (!string.IsNullOrEmpty(expandStatus) && expandStatus == st);
                        int takeCount = isExpanded ? 50 : 2;

                        vm.OrderGroups.Add(new OrderGroupItemVM
                        {
                            StatusName = st,
                            TotalCount = ordersInSt.Count,
                            IsExpanded = isExpanded,
                            Orders = ordersInSt.Take(takeCount).ToList()
                        });
                    }
                }

                // Nếu có đơn mang trạng thái khác ngoài danh sách chuẩn
                var otherOrders = filteredOrders.Where(o => !standardStatuses.Contains(o.OrderStatusDisplay)).ToList();
                foreach (var grp in otherOrders.GroupBy(o => o.OrderStatusDisplay))
                {
                    bool isExpanded = (!string.IsNullOrEmpty(expandStatus) && expandStatus == grp.Key);
                    int takeCount = isExpanded ? 50 : 2;

                    vm.OrderGroups.Add(new OrderGroupItemVM
                    {
                        StatusName = grp.Key,
                        TotalCount = grp.Count(),
                        IsExpanded = isExpanded,
                        Orders = grp.Take(takeCount).ToList()
                    });
                }
            }
            else
            {
                // Từng tab trạng thái riêng lẻ: Chỉ lọc đúng trạng thái đó
                var ordersInSt = filteredOrders.Where(o => o.OrderStatusDisplay == activeStatus).ToList();
                bool isExpanded = (!string.IsNullOrEmpty(expandStatus) && expandStatus == activeStatus);
                int takeCount = isExpanded ? 50 : 2;

                vm.OrderGroups.Add(new OrderGroupItemVM
                {
                    StatusName = activeStatus,
                    TotalCount = ordersInSt.Count,
                    IsExpanded = isExpanded,
                    Orders = ordersInSt.Take(takeCount).ToList()
                });
            }

            return View(vm);
        }

        // Hỗ trợ route OrderHistory tương thích với các link cũ
        [Authorize]
        public ActionResult OrderHistory(string search, string status)
        {
            return RedirectToAction("MyOrder", new { searchTerm = search, status = status });
        }

        // Action Mua lại: Thêm các sản phẩm trong đơn hàng vào giỏ với số lượng = 1
        [Authorize]
        public ActionResult BuyAgain(int orderId)
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : (Session["Username"] != null ? Session["Username"].ToString() : "");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Include(o => o.OrderDetail.Select(od => od.Product))
                                 .SingleOrDefault(o => o.OrderID == orderId && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }

            var cartService = new CartService(Session);
            var cart = cartService.GetCart();

            foreach (var d in order.OrderDetail)
            {
                var pro = d.Product ?? db.Products.Find(d.ProductID);
                if (pro != null)
                {
                    string catName = (pro.Category != null) ? pro.Category.CategoryName : "Khác";
                    cart.AddItem(pro.ProductID, pro.ProductImage, pro.ProductName, pro.ProductPrice, 1, catName);
                }
            }

            TempData["SuccessMessage"] = $"Đã thêm các sản phẩm từ đơn hàng #{orderId} vào giỏ hàng.";
            return RedirectToAction("Index", "Cart");
        }

        // GET / POST: Order/CancelOrder/5 - Hủy đơn hàng và HOÀN LẠI SỐ LƯỢNG TỒN KHO
        [Authorize]
        public ActionResult CancelOrder(int id)
        {
            string username = User.Identity.IsAuthenticated ? User.Identity.Name : (Session["Username"] != null ? Session["Username"].ToString() : "");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Include(o => o.OrderDetail).SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.PaymentStatus != "Đã hủy" && order.OrderStatus != "Đã hủy")
            {
                // Hoàn lại số lượng tồn kho vào bảng Product
                foreach (var detail in order.OrderDetail)
                {
                    var pro = db.Products.Find(detail.ProductID);
                    if (pro != null)
                    {
                        pro.Quantity += detail.Quantity;
                    }
                }
                order.PaymentStatus = "Đã hủy";
                order.OrderStatus = "Đã hủy";
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{order.OrderID} thành công và hoàn trả số lượng vào kho.";
            }
            else
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã được hủy trước đó.";
            }

            return RedirectToAction("MyOrder");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
