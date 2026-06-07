namespace CRM.Models
{
    public class BillInfo
    {
        public ulong ClientId { get; set; }
        public decimal? Balance { get; set; }
        public string BankAccount { get; set; }

        public virtual Client Client { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}