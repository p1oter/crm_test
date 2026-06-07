namespace CRM.Models
{
    public class Payment
    {
        public ulong Id { get; set; }
        public string BankAccount { get; set; }
        public decimal? Amount { get; set; }
        public DateTime CreatedT { get; set; }

        public virtual BillInfo BillInfo { get; set; }
    }
}