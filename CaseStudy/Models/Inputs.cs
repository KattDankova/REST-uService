using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{

    //Ojekty využité pro "úhlednější" vstupní parametry API requestů
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

    //Sjednocení, aby všechny parametry, které nejsou součástí URL, byly součástí objektu
    public class IDOfOrder
    {
        [Required]
        public string IDOrder { get; set; }
    }

    //Informace o zaplacení do Kafky
    public class MessageInput
    {
        [Required]
        public string IDOrder { get; set; }
        [Required]
        public bool Paid { get; set; }
    }
}
