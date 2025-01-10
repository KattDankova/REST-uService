using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class Order
    {
        [Key]
        public int IDOrder { get; set; }
        public int OrderNumber { get; set; }
        public required string CustomerName { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public required List<Item> Items { get; set; }

    }

    public enum OrderStatus
    {
        New = 0,
        Paid = 1,
        Canceled = 2
    }

    public class OrderInput
    {
        public required string CustomerName { get; set; }
        public required List<Item> Items { get; set; }
    }
}
