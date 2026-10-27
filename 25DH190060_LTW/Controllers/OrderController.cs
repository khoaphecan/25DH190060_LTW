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

                // Tạo đối tượng Order và lưu vào DB
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalValue(),
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    DeliveryMethod = model.ShippingMethod,
                    AddressDelivery = !string.IsNullOrEmpty(model.ShippingAddress) ? model.ShippingAddress : customer.CustomerAddress
                };
                db.Orders.Add(order);
                db.SaveChanges();

                // Tạo các chi tiết đơn hàng (OrderDetail)
                foreach (var item in cart.Items)
                {
                    var detail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    };
                    db.OrderDetails.Add(detail);
                }
                db.SaveChanges();

                // Xóa giỏ hàng sau khi đặt hàng thành công
                Session["Cart"] = null;

                // Điều hướng tới trang Xác nhận đơn hàng
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            // Nếu model không hợp lệ, nạp lại CartItems để hiển thị lại
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
