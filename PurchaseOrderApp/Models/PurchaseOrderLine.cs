// Models/PurchaseOrderLine.cs
using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApp.Models
{
    public class PurchaseOrderLine
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20)]
        public string PurchId { get; set; }

        [Required]
        [StringLength(20)]
        public string ItemId { get; set; }

        [Required]
        public double Quantity { get; set; }

        [Required]
        public double LineAmount { get; set; }
    }
}