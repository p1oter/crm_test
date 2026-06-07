namespace CRM.Models
{
    public class InvoiceLine
    {
        public ulong Id { get; set; }
        public ulong? InvoiceId { get; set; }
        public ulong ServiceId { get; set; }
        public ulong EmployeeId { get; set; }
        public ulong ClientId { get; set; }
        public decimal? Price { get; set; }
        public uint Amount { get; set; }

        public virtual Invoice Invoice { get; set; }
        public virtual Service Service { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual Client Client { get; set; }
    }
}