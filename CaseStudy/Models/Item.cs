using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class Item
    {
        [Key]
        public int IDItem { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [Range(0, double.MaxValue)]
        public double Price { get; set; }
        public ICollection<OrderItems>? Orders { get; set; }
    }
}
