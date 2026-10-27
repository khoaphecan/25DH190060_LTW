using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1;

        // Tính giá trị tạm tính = quantity * product.ProductPrice
        public decimal estimatedValue => quantity * (product != null ? product.ProductPrice : 0);

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 3;

        // danh sách sản phẩm cùng danh mục
        public List<Product> RelatedProducts { get; set; }

        // danh sách sản phẩm bán chạy nhất cùng danh mục
        public IPagedList<Product> TopProducts { get; set; }
    }
}
