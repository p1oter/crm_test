namespace CRM.Models
{
    public class Employee
    {
        public ulong Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public ulong? SupervisorId { get; set; }

        public virtual Employee Supervisor { get; set; }
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public virtual GuiUser GuiUser { get; set; }
        public virtual ICollection<EmployeeSkills> Skills { get; set; } = new List<EmployeeSkills>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    }
}