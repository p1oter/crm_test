namespace CRM.Models
{
    public class Service
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public ulong ServiceGroupId { get; set; }

        public virtual ServiceGroup ServiceGroup { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    }
}