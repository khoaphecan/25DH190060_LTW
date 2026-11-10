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
            string username = User.Identity.Name;
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

            string username = User.Identity.Name;
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
                // Thiết lập PaymentStatus dựa trên PaymentMethod
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt":
                        paymentStatus = "Thanh toán tiền mặt";
                        break;
                    case "Paypal":
                        paymentStatus = "Thanh toán paypal";
                        break;
                    case "Mua trước trả sau":
                        paymentStatus = "Trả góp";
                        break;
                    default:
                        paymentStatus = "Chưa thanh toán";
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
                        UnitPrice = item.UnitPrice
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

        // GET: Order/MyOrder hoặc OrderHistory
        [Authorize]
        public ActionResult MyOrder(string search, string status)
        {
            return OrderHistory(search, status);
        }

        [Authorize]
        public ActionResult OrderHistory(string search, string status)
        {
            string username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.OrderDetail.Select(d => d.Product))
                .Where(o => o.CustomerID == customer.CustomerID)
                .AsQueryable();

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                orders = orders.Where(o => o.PaymentStatus.Contains(status));
            }

            // Tìm kiếm theo Mã đơn hàng hoặc Tên sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                int orderId;
                if (int.TryParse(search, out orderId))
                {
                    orders = orders.Where(o => o.OrderID == orderId ||
                                               o.OrderDetail.Any(d => d.Product.ProductName.Contains(search)));
                }
                else
                {
                    orders = orders.Where(o => o.OrderDetail.Any(d => d.Product.ProductName.Contains(search)));
                }
                ViewBag.CurrentSearch = search;
            }

            ViewBag.CurrentStatus = status ?? "Tất cả";
            var result = orders.OrderByDescending(o => o.OrderID).ToList();
            return View("MyOrder", result);
        }

        // GET / POST: Order/CancelOrder/5 - Hủy đơn hàng và HOÀN LẠI SỐ LƯỢNG TỒN KHO
        [Authorize]
        public ActionResult CancelOrder(int id)
        {
            string username = User.Identity.Name;
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

            if (order.PaymentStatus != "Đã hủy" && order.PaymentStatus != "Cancelled")
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
