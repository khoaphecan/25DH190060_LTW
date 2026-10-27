using System.Data.Entity;

namespace _25DH190060_LTW.Models
{
    public partial class MyStoreEntities
    {
        public DbSet<Category> Categories => Category;
        public DbSet<Customer> Customers => Customer;
        public DbSet<Order> Orders => Order;
        public DbSet<OrderDetail> OrderDetails => OrderDetail;
        public DbSet<Product> Products => Product;
        public DbSet<User> Users => User;
    }
}
