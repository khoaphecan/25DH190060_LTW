using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
            var stats = db.Category
                .Select(c => new CategoryStatsVM
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    ProductCount = c.Product.Count(),
                    MaxPrice = c.Product.Any() ? c.Product.Max(p => p.ProductPrice) : 0,
                    MinPrice = c.Product.Any() ? c.Product.Min(p => p.ProductPrice) : 0,
                    AvgPrice = c.Product.Any() ? c.Product.Average(p => p.ProductPrice) : 0
                })
                .ToList();

            return View(stats);
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
