using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items => items;

        // danh sách các sản phẩm trong giỏ hàng đã được nhóm theo Category Name
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => !string.IsNullOrEmpty(i.Category) ? i.Category : "Khác").ToList();

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;

        // danh sách các sản phẩm cùng danh mục với các sản phẩm trong giỏ
        public IPagedList<Product> SimilarProducts { get; set; }

        // Thêm sản phẩm vào giỏ
        public void AddItem(int productid, string productimage, string productname, decimal unitprice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productid);
            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productid,
                    ProductImage = productimage,
                    ProductName = productname,
                    UnitPrice = unitprice,
                    Quantity = quantity,
                    Category = category
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        // Xóa sản phẩm khỏi giỏ
        public void RemoveItem(int productid)
        {
            items.RemoveAll(i => i.ProductID == productid);
        }

        // Tính tổng giá trị giỏ hàng
        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        // Làm trống giỏ hàng
        public void Clear()
        {
            items.Clear();
        }

        // cập nhật số lượng của sản phẩm đã chọn
        public void UpdateQuantity(int productid, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productid);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    items.Remove(item);
                }
            }
        }
    }
}
