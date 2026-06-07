namespace CRM.Models
{
    public class Invoice
    {
        public ulong Id { get; set; }
        public ulong ClientId { get; set; }
        public DateTime CreatedT { get; set; }
        public DateTime? DueT { get; set; }
        public bool SentToClient { get; set; }

        public virtual Client Client { get; set; }
        public virtual ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    }
}