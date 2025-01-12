using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CaseStudy.Models
{
    public class Order
    {
        //Přemýšlela jsem, zda použít Číslo objednávky jako identifikátor objednávky, ale přišlo mi, že Číslo objednávky je identifikátor přístupný pro uživatele, zatímco ID objednávky by v některých případech být přístupné nemělo
        //(Proto ID objednávky používám u přijetí informace o zaplacení a obnovení stavu objednávky, naopak pro získání detailu objednávky pro uživatele request vyžaduje Číslo objednávky)
        [Key]
        public Guid IDOrder { get; set; }
        [Required]
        [Range(100000, 999999)]
        public int OrderNumber { get; set; } = 100000001;
        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public ICollection<OrderItems> Items { get; set; } = [];

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
}
