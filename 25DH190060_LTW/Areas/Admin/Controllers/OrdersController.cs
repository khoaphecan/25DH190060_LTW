using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;

namespace _25DH190060_LTW.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Orders/Index
        public ActionResult Index(string customerName, string paymentStatus, DateTime? orderDate, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            // Lọc theo tên khách hàng
            if (!string.IsNullOrEmpty(customerName))
            {
                orders = orders.Where(o => o.Customer != null && o.Customer.CustomerName.Contains(customerName));
            }

            // Lọc theo trạng thái thanh toán
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentStatus == paymentStatus);
            }

            // Lọc theo ngày đặt hàng (dùng DbFunctions.TruncateTime tránh lỗi Linq to Entities)
            if (orderDate.HasValue)
            {
                DateTime targetDate = orderDate.Value.Date;
                orders = orders.Where(o => DbFunctions.TruncateTime(o.OrderDate) == targetDate);
            }

            // Sắp xếp mới nhất lên đầu
            orders = orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderID);

            // Phân trang 15 đơn hàng / trang
            int pageNumber = page ?? 1;
            int pageSize = 15;
            var pagedOrders = orders.ToPagedList(pageNumber, pageSize);

            var model = new OrderSearchVM
            {
                CustomerName = customerName,
                PaymentStatus = paymentStatus,
                OrderDate = orderDate,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Orders = pagedOrders
            };

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                foreach (var item in model.PaymentStatusList)
                {
                    item.Selected = (item.Value == paymentStatus);
                }
            }

            return View(model);
        }

        // GET: Admin/Orders/DetailedOrder/5
        public ActionResult DetailedOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetail.Select(od => od.Product))
                .SingleOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // GET: Admin/Orders/Details/5 (hỗ trợ điều hướng tới DetailedOrder)
        public ActionResult Details(int? id)
        {
            return DetailedOrder(id);
        }

        // GET: Admin/Orders/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders.Include(o => o.Customer).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string paymentStatus, string orderStatus)
        {
            Order order = db.Orders.Include(o => o.OrderDetail).SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra nếu chuyển sang "Đã hủy" thì hoàn trả lại số lượng tồn kho
            bool wasCancelled = order.PaymentStatus == "Đã hủy" || order.OrderStatus == "Đã hủy";
            bool nowCancelled = paymentStatus == "Đã hủy" || orderStatus == "Đã hủy";

            if (!wasCancelled && nowCancelled)
            {
                foreach (var d in order.OrderDetail)
                {
                    var pro = db.Products.Find(d.ProductID);
                    if (pro != null)
                    {
                        pro.Quantity += d.Quantity;
                    }
                }
            }

            order.PaymentStatus = paymentStatus;
            order.OrderStatus = orderStatus;

            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("DetailedOrder", new { id = order.OrderID });
        }

        // GET: Admin/Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Order order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetail.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Include(o => o.OrderDetail).FirstOrDefault(o => o.OrderID == id);
            if (order != null)
            {
                // Nếu đơn hàng chưa bị hủy thì khi xóa hoàn trả lại số lượng tồn kho
                if (order.PaymentStatus != "Đã hủy" && order.OrderStatus != "Đã hủy")
                {
                    foreach (var d in order.OrderDetail)
                    {
                        var pro = db.Products.Find(d.ProductID);
                        if (pro != null)
                        {
                            pro.Quantity += d.Quantity;
                        }
                    }
                }

                var details = db.OrderDetails.Where(d => d.OrderID == id).ToList();
                foreach (var d in details)
                {
                    db.OrderDetails.Remove(d);
                }
                db.Orders.Remove(order);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
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
