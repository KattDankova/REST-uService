using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CaseStudy.Models
{
    public class Item
    {
        [Key]
        public int IDItem { get; set; }
        public required string Name { get; set; }
        public required double Price { get; set; }
        public ICollection<Order> Orders { get; set; }
    }

    public class ItemInput
    {
        public required int IDItem { get; set; }
        public required int Quantity { get; set; }
    }
}
