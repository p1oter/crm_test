using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Reservation
    {
        public ulong Id { get; set; }

        [Required(ErrorMessage = "Data i godzina rezerwacji s¹ wymagane")]
        public DateTime StartT { get; set; }

        [Required(ErrorMessage = "Pracownik jest wymagany")]
        public ulong EmployeeId { get; set; }

        [Required(ErrorMessage = "Klient jest wymagany")]
        public ulong ClientId { get; set; }

        [Required(ErrorMessage = "Us³uga jest wymagana")]
        public ulong ServiceId { get; set; }

        // Navigation properties
        public virtual Employee? Employee { get; set; }
        public virtual Client? Client { get; set; }
        public virtual Service? Service { get; set; }
    }
}