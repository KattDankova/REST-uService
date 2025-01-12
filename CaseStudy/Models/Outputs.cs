namespace CaseStudy.Models
{
    // Objekty využité pro "úhlednější" výstup API
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

    public class ItemOutput
    {
        public int IDItem { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double CalculatedPrice { get; set; }
    }
}
