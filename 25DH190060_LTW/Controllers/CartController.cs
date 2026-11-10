using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH190060_LTW.Models;
using _25DH190060_LTW.Models.ViewModel;
using PagedList;

namespace _25DH190060_LTW.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Hàm lấy dịch vụ giỏ hàng
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // Hiển thị giỏ hàng đã gom nhóm sản phẩm theo danh mục và có sản phẩm tương tự
        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();
            var products = db.Products.Include("Category").ToList();
            var similarProducts = new List<Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                similarProducts = products.Where(p => cart.Items.Any(ci => ci.Category == (p.Category != null ? p.Category.CategoryName : ""))
                                                   && !cart.Items.Any(ci => ci.ProductID == p.ProductID)).ToList();
            }

            // code liên quan tới phân trang
            int pageNumber = page ?? 1;
            int pageSize = cart.PageSize > 0 ? cart.PageSize : 6;
            cart.SimilarProducts = similarProducts.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(cart);
        }

        // Thêm sản phẩm vào giỏ (có kiểm tra tồn kho)
        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Include("Category").FirstOrDefault(p => p.ProductID == id);
            if (product != null)
            {
                var cartService = GetCartService();
                var currentItem = cartService.GetCart().Items.FirstOrDefault(i => i.ProductID == id);
                int currentQty = currentItem != null ? currentItem.Quantity : 0;

                if (currentQty + quantity > product.Quantity)
                {
                    TempData["StockError"] = $"Sản phẩm '{product.ProductName}' hiện chỉ còn {product.Quantity} trong kho!";
                    return RedirectToAction("Index");
                }

                string categoryName = product.Category != null ? product.Category.CategoryName : "Khác";
                cartService.GetCart().AddItem(product.ProductID, product.ProductImage, product.ProductName, product.ProductPrice, quantity, categoryName);
            }
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ
        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            cartService.GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        // Làm trống giỏ hàng
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng (có kiểm tra tồn kho)
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var product = db.Products.Find(id);
            if (product != null && quantity > product.Quantity)
            {
                TempData["StockError"] = $"Sản phẩm '{product.ProductName}' hiện chỉ còn {product.Quantity} trong kho!";
                return RedirectToAction("Index");
            }

            var cartService = GetCartService();
            cartService.GetCart().UpdateQuantity(id, quantity);
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
