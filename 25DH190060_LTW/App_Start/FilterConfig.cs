using System.Web;
using System.Web.Mvc;

namespace _25DH190060_LTW
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            // Tạm thời comment HandleErrorAttribute để tránh nuốt exception nội bộ và chuyển hướng sang /Shared/Error
            // filters.Add(new HandleErrorAttribute());
        }
    }
}
