using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Service
    {
        public ulong Id { get; set; }

        [Required(ErrorMessage = "Nazwa us³ugi jest wymagana")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0, double.MaxValue, ErrorMessage = "Cena musi byæ wiêksza lub równa 0")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Kategoria jest wymagana")]
        public ulong? ServiceGroupId { get; set; }

        // Navigation properties
        public virtual ServiceGroup? ServiceGroup { get; set; }
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}