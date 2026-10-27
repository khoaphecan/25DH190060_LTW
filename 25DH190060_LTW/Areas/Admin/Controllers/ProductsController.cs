using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;
using PagedList;

namespace _25DH190060_LTW.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/Products
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new ProductSearchVM();
            var products = db.Product.Include(p => p.Category).AsQueryable();

            // Tìm kiếm sản phẩm dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductDecription.Contains(searchTerm) ||
                    p.Category.CategoryName.Contains(searchTerm));
            }

            // Tìm kiếm sản phẩm dựa trên giá tối thiểu
            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice.Value;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }

            // Tìm kiếm sản phẩm dựa trên giá tối đa
            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice.Value;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            // Áp dụng sắp xếp dựa trên lựa chọn của người dùng
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    // Mặc định sắp xếp theo tên
                    products = products.OrderBy(p => p.ProductName);
                    break;
            }
            model.SortOrder = sortOrder;

            // Đoạn code liên quan tới phân trang
            // Lấy số trang hiện tại (mặc định là trang 1 nếu không có giá trị)
            int pageNumber = page ?? 1;
            int pageSize = 10; // Số sản phẩm mỗi trang (có thể đổi thành 4 hoặc 10)
            model.PageNumber = pageNumber;
            model.PageSize = pageSize;

            model.Products = products.ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Thêm đoạn code để lấy đường link của ảnh đã upload và lưu ảnh vào thư mục Content/images
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    string filename = Path.GetFileName(product.UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.ProductImage = savePath + filename;

                    string physicalPath = Server.MapPath(savePath);
                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }
                    product.UploadImg.SaveAs(Path.Combine(physicalPath, filename));
                }
                else if (string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = "~/Content/images/default_img.png";
                }

                db.Product.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDecription,ProductPrice,ProductImage,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Thêm đoạn code để lấy đường link của ảnh đã upload và lưu ảnh vào thư mục Content/images
                if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
                {
                    string filename = Path.GetFileName(product.UploadImg.FileName);
                    string savePath = "~/Content/images/";
                    product.ProductImage = savePath + filename;

                    string physicalPath = Server.MapPath(savePath);
                    if (!Directory.Exists(physicalPath))
                    {
                        Directory.CreateDirectory(physicalPath);
                    }
                    product.UploadImg.SaveAs(Path.Combine(physicalPath, filename));
                }

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Category, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Product.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Product.Find(id);
            db.Product.Remove(product);
            db.SaveChanges();
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
