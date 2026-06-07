using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Employee
    {
        public ulong Id { get; set; }

        [Required(ErrorMessage = "Imiê jest wymagane")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Stanowisko jest wymagane")]
        [StringLength(100)]
        public string? Position { get; set; }

        public ulong? SupervisorId { get; set; }

        // Navigation properties
        public virtual Employee? Supervisor { get; set; }
        public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
        public virtual GuiUser? GuiUser { get; set; }
        public virtual ICollection<EmployeeSkills> Skills { get; set; } = new List<EmployeeSkills>();
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}