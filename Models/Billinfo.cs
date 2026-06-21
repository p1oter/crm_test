using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CRM.Models
{
    public class BillInfo
    {
        [Key]
        public ulong ClientId { get; set; }

        // BALANCE w DB mo¿e byæ NULL -> u¿ywamy nullable decimal
        public decimal? Balance { get; set; }

        [Required]
        public string? BankAccount { get; set; }

        // navigation
        [ValidateNever]
        public virtual Client? Client { get; set; }

        [ValidateNever]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}