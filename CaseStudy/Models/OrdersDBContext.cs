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

            modelBuilder.Entity<OrderItems>()
                .HasKey(sc => new { sc.IDOrder, sc.IDItem });

            modelBuilder.Entity<OrderItems>()
                .HasOne(sc => sc.Order)
                .WithMany(s => s.Items)
                .HasForeignKey(sc => sc.IDOrder);

            modelBuilder.Entity<OrderItems>()
                .HasOne(sc => sc.Item)
                .WithMany(c => c.Orders)
                .HasForeignKey(sc => sc.IDItem);


            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 1, Name = "Jablka", Price = 20 });
            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 2, Name = "Hrušky", Price = 30 });
            modelBuilder.Entity<Item>().HasData(new Item { IDItem = 3, Name = "Banány", Price = 40 });
        }

    }
}
