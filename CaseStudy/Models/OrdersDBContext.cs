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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<OrderItems>()
                .HasKey(sc => new { sc.IDOrder, sc.IDItem });

            //Jedna objednávka může obsahovat více typů zboží
            modelBuilder.Entity<OrderItems>()
                .HasOne(sc => sc.Order)
                .WithMany(s => s.Items)
                .HasForeignKey(sc => sc.IDOrder);

            //Jedno zboží si může objednat více lidí
            modelBuilder.Entity<OrderItems>()
                .HasOne(sc => sc.Item)
                .WithMany(c => c.Orders)
                .HasForeignKey(sc => sc.IDItem);

            //Předvytvořená náhodná data za účely testování
            modelBuilder.Entity<Item>().HasData(
                new Item { IDItem = 1, Name = "Jablka", Price = 10 },
                new Item { IDItem = 2, Name = "Hrušky", Price = 20 },
                new Item { IDItem = 3, Name = "Banány", Price = 30 }
            );
        }
    }
}
