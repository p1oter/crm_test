// Models/Invoice.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Invoice
    {
        public ulong Id { get; set; }

        [Required]
        public ulong ClientId { get; set; }

        [Required]
        public DateTime CreatedT { get; set; }

        public DateTime? DueT { get; set; }

        public bool SentToClient { get; set; }

        public virtual Client? Client { get; set; }

        // change from ICollection<InvoiceLine> to List<InvoiceLine>
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }
}