using System;
using System.Linq;
using System.Web.Mvc;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;

namespace _25DH190060_LTW.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Home
        public ActionResult Index()
        {
            // Thống kê số lượng, giá cao nhất, thấp nhất, trung bình theo từng loại hàng
            var stats = db.Product
                .GroupBy(p => p.Category.CategoryName)
                .Select(g => new CategoryStatVM
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    MaxPrice = g.Max(p => p.ProductPrice),
                    MinPrice = g.Min(p => p.ProductPrice),
                    AvgPrice = g.Average(p => p.ProductPrice)
                })
                .ToList();

            return View(stats);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}