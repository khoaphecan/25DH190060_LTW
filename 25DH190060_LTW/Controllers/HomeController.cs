using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;
using PagedList;

namespace _25DH190060_LTW.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.Include(p => p.Category).Include(p => p.OrderDetail).AsQueryable();

            // Tìm kiếm sản phẩm dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDecription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            // code liên quan tới phân trang
            int pageNumber = page ?? 1;
            int pageSize = 6;

            // lấy top 10 sản phẩm bán chạy nhất (nổi bật)
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetail.Count()).Take(10).ToList();

            // lấy 20 sản phẩm bán ế nhất (mới) và phân trang 6 sp/trang
            model.NewProducts = products.OrderBy(p => p.OrderDetail.Count()).Take(20).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Home/ProductList
        public ActionResult ProductList(int? categoryId, string searchTerm, int? page)
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.OrderDetail).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
                ViewBag.CurrentCategory = db.Categories.Find(categoryId.Value)?.CategoryName;
                ViewBag.CategoryID = categoryId.Value;
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDecription.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;
            ViewBag.Categories = db.Categories.ToList();

            return View(products.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize));
        }

        // GET: Home/ProductDetail/5 hoặc ProductDetails/5
        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            return ProductDetails(id, quantity, page);
        }

        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            // lấy tất cả các sản phẩm cùng danh mục
            var products = db.Products.Include(p => p.Category).Include(p => p.OrderDetail)
                .Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID)
                .AsQueryable();

            var model = new ProductDetailsVM();
            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetail.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            return View("ProductDetails", model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
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
