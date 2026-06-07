namespace CRM.Models
{
    public class Reservation
    {
        public ulong Id { get; set; }
        public DateTime StartT { get; set; }
        public ulong EmployeeId { get; set; }
        public ulong ClientId { get; set; }
        public ulong ServiceId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Client Client { get; set; }
        public virtual Service Service { get; set; }
    }
}