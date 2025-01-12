using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class OrderInput
    {
        [Required]
        public string CustomerName { get; set; }
        [Required]
        public ICollection<ItemInput> Items { get; set; }
    }

    public class ItemInput
    {
        [Required]
        public int IDItem { get; set; }
        [Required]
        public int Quantity { get; set; }
    }

    public class IDOfOrder
    {
        [Required]
        public string IDOrder { get; set; }
    }

    public class MessageInput
    {
        [Required]
        public string IDOrder { get; set; }
        [Required]
        public bool Paid { get; set; }
    }
}
