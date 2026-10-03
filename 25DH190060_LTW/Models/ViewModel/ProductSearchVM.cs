using System.Collections.Generic;
using PagedList;
using _25DH190060_LTW.Models;

namespace _25DH190060_LTW.Models.ViewModel
{
    public class ProductSearchVM
    {
        // Tiêu chí tìm kiếm
        public string SearchTerm { get; set; }

        // Tiêu chí lọc giá
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Thứ tự sắp xếp
        public string SortOrder { get; set; }

        // Phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        // Danh sách sản phẩm dạng phân trang
        public IPagedList<Product> Products { get; set; }
    }
}