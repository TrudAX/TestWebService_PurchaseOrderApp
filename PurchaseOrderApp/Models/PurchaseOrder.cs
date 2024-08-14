using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PurchaseOrderApp.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20)]
        public string PurchId { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderAccount { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime UpdatedAt { get; set; }

        public List<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
    }
}