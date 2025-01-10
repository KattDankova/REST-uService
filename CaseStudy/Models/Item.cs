using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class Item
    {
        [Key]
        public int IDItem { get; set; }
        public required string Name { get; set; }
        public required int Quantity { get; set; }
        public required double PricePerOne { get; set; }
    }
}
