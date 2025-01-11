using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class Order
    {
        [Key]
        public Guid IDOrder { get; set; }
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public ICollection<OrderItems> Items { get; set; }

    }

    public enum OrderStatus
    {
        [Description("Nová")]
        New = 0,
        [Description("Zaplacená")]
        Paid = 1,
        [Description("Zrušená")]
        Canceled = 2
    }

    public class OrderInput
    {
        public string CustomerName { get; set; }
        public ICollection<ItemInput> Items { get; set; }
    }

    public class OrderOutput
    {
        public string IDOrder { get; set; }
        public int OrderNumber { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public double SumPrice { get; set; }
        public ICollection<ItemOutput> Items { get; set; }
    }
}
