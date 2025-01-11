using Microsoft.EntityFrameworkCore;
using System.Configuration;
using static System.Net.WebRequestMethods;

namespace CaseStudy.Models
{
    public class OrdersDBContext : DbContext
    {
        public OrdersDBContext(DbContextOptions<OrdersDBContext> options) : base(options) { }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<OrderItems> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("CaseStudyOrders");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Order>()
           .HasMany(i => i.Items)
           .WithMany(o => o.Orders)
           .UsingEntity<OrderItems>(
               x => x.HasOne(oi => oi.Item).WithMany().HasForeignKey(oi => oi.IDItem),
               x => x.HasOne(oi => oi.Order).WithMany().HasForeignKey(oi => oi.IDOrder)
           );

            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 1, Name = "Jablka", Price = 20 });
            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 2, Name = "Hrušky", Price = 30 });
            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 3, Name = "Banány", Price = 40 });
        }

    }
}
