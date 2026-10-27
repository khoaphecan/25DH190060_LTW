using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;

namespace _25DH190060_LTW.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                // Nếu chưa tồn tại thì tạo bản ghi thông tin tài khoản trong bảng User
                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password, // Lưu ý: Nên mã hóa mật khẩu trước khi lưu
                    UserRole = "C" // C: customer, A: admin theo database2024
                };
                db.Users.Add(user);

                // Và tạo bản ghi thông tin khách hàng trong bảng Customer
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username
                };
                db.Customers.Add(customer);

                // Lưu thông tin tài khoản và thông tin khách hàng vào CSDL
                db.SaveChanges();

                // Tự động đăng nhập hoặc chuyển hướng về trang chủ/đăng nhập
                Session["Username"] = user.Username;
                Session["UserRole"] = user.UserRole;
                FormsAuthentication.SetAuthCookie(user.Username, false);

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username && u.Password == model.Password);
                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole != null ? user.UserRole.Trim() : "C";

                    // Lưu thông tin xác thực người dùng vào cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    // Nếu là Admin, có thể chuyển tới Admin site
                    string role = user.UserRole != null ? user.UserRole.Trim() : "";
                    if (role.Equals("A", StringComparison.OrdinalIgnoreCase) || role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/ProfileInfo
        [Authorize]
        public ActionResult ProfileInfo()
        {
            string username = User.Identity.Name;
            if (string.IsNullOrEmpty(username) && Session["Username"] != null)
            {
                username = Session["Username"].ToString();
            }

            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            var model = new ProfileInfoVM
            {
                Username = user.Username,
                CustomerID = customer != null ? customer.CustomerID : 0,
                CustomerName = customer != null ? customer.CustomerName : "",
                CustomerPhone = customer != null ? customer.CustomerPhone : "",
                CustomerEmail = customer != null ? customer.CustomerEmail : "",
                CustomerAddress = customer != null ? customer.CustomerAddress : ""
            };

            return View(model);
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileInfoVM model)
        {
            string username = User.Identity.Name;
            var customer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (customer != null)
            {
                customer.CustomerName = model.CustomerName;
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerEmail = model.CustomerEmail;
                customer.CustomerAddress = model.CustomerAddress;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            return RedirectToAction("ProfileInfo");
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ProfileInfoVM model)
        {
            string username = User.Identity.Name;
            var user = db.Users.SingleOrDefault(u => u.Username == username);
            if (user != null)
            {
                if (user.Password.Trim() != model.CurrentPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu hiện tại không chính xác!";
                }
                else if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword != model.ConfirmNewPassword)
                {
                    TempData["ErrorMessage"] = "Mật khẩu mới không hợp lệ hoặc xác nhận không khớp!";
                }
                else
                {
                    user.Password = model.NewPassword;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                }
            }
            return RedirectToAction("ProfileInfo");
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
