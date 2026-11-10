using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH190060_LTW.Models;

namespace _25DH190060_LTW.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Customers
        public ActionResult Index()
        {
            // Chỉ hiển thị những người dùng thực sự là Khách hàng (UserRole = 'C' hoặc bắt đầu bằng 'C')
            var customers = db.Customer.Include(c => c.User)
                .Where(c => c.User == null || c.User.UserRole.StartsWith("C"))
                .ToList();
            return View(customers);
        }

        // GET: Admin/Customers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Include(c => c.Order).Include(c => c.User).FirstOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            // Chỉ lấy tài khoản khách hàng (Role 'C') chưa từng được gán cho Customer nào
            var usedUsernames = db.Customer.Select(c => c.Username).ToList();
            var availableUsers = db.User
                .Where(u => u.UserRole.StartsWith("C") && !usedUsernames.Contains(u.Username))
                .ToList();
            ViewBag.Username = new SelectList(availableUsers, "Username", "Username");
            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Username))
            {
                ModelState.AddModelError("Username", "Vui lòng chọn tài khoản liên kết.");
            }
            else
            {
                var user = db.User.FirstOrDefault(u => u.Username == customer.Username);
                if (user == null)
                {
                    ModelState.AddModelError("Username", "Tài khoản này không tồn tại trong hệ thống.");
                }
                else if (user.UserRole.StartsWith("A"))
                {
                    ModelState.AddModelError("Username", "Tài khoản quản trị viên (Admin) không được tạo hồ sơ khách hàng.");
                }
                else if (db.Customer.Any(c => c.Username == customer.Username))
                {
                    ModelState.AddModelError("Username", "Tài khoản này đã được gán cho một khách hàng khác.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Customer.Add(customer);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm khách hàng mới thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                }
            }

            var usedUsernames = db.Customer.Select(c => c.Username).ToList();
            var availableUsers = db.User
                .Where(u => u.UserRole.StartsWith("C") && (!usedUsernames.Contains(u.Username) || u.Username == customer.Username))
                .ToList();
            ViewBag.Username = new SelectList(availableUsers, "Username", "Username", customer.Username);
            return View(customer);
        }

        // GET: Admin/Customers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            var usedUsernames = db.Customer.Where(c => c.CustomerID != customer.CustomerID).Select(c => c.Username).ToList();
            var availableUsers = db.User
                .Where(u => u.UserRole.StartsWith("C") && (!usedUsernames.Contains(u.Username) || u.Username == customer.Username))
                .ToList();
            ViewBag.Username = new SelectList(availableUsers, "Username", "Username", customer.Username);
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CustomerID,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Username")] Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Username))
            {
                ModelState.AddModelError("Username", "Vui lòng chọn tài khoản liên kết.");
            }
            else
            {
                var user = db.User.FirstOrDefault(u => u.Username == customer.Username);
                if (user == null)
                {
                    ModelState.AddModelError("Username", "Tài khoản này không tồn tại trong hệ thống.");
                }
                else if (user.UserRole.StartsWith("A"))
                {
                    ModelState.AddModelError("Username", "Tài khoản quản trị viên (Admin) không được tạo hồ sơ khách hàng.");
                }
                else if (db.Customer.Any(c => c.Username == customer.Username && c.CustomerID != customer.CustomerID))
                {
                    ModelState.AddModelError("Username", "Tài khoản này đã được gán cho một khách hàng khác.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(customer).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Cập nhật thông tin khách hàng thành công!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                }
            }

            var usedUsernames = db.Customer.Where(c => c.CustomerID != customer.CustomerID).Select(c => c.Username).ToList();
            var availableUsers = db.User
                .Where(u => u.UserRole.StartsWith("C") && (!usedUsernames.Contains(u.Username) || u.Username == customer.Username))
                .ToList();
            ViewBag.Username = new SelectList(availableUsers, "Username", "Username", customer.Username);
            return View(customer);
        }

        // GET: Admin/Customers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customer.Include(c => c.Order).FirstOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            return View(customer);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customer customer = db.Customer.Include(c => c.Order).FirstOrDefault(c => c.CustomerID == id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra nếu khách hàng đã có đơn hàng trong hệ thống
            if (customer.Order != null && customer.Order.Any())
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng này vì đã có lịch sử đơn hàng trong hệ thống!";
                return RedirectToAction("Index");
            }

            try
            {
                db.Customer.Remove(customer);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Xóa khách hàng thành công!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Không thể xóa khách hàng này do ràng buộc dữ liệu liên quan!";
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
