using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseStudy.Models
{
    public class Item
    {
        [Key]
        public int IDItem { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public ICollection<OrderItems> Orders { get; set; }
    }

    public class ItemInput
    {
        public int IDItem { get; set; }
        public int Quantity { get; set; }
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
