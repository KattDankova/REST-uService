namespace CaseStudy.Models
{
    public class OrderItems
    {
        public int IDOrder { get; set; }
        public Order Order { get; set; }
        public int IDItem { get; set; }
        public Item Item { get; set; }
        public int Quantity { get; set; }
    }
}
