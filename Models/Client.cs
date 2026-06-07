using System.ComponentModel.DataAnnotations;

namespace CRM.Models
{
    public class Client
    {
        public ulong Id { get; set; }

        [Required(ErrorMessage = "Imiê jest wymagane")]
        [StringLength(100)]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Nazwisko jest wymagane")]
        [StringLength(100)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email jest wymagany")]
        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Ulica jest wymagana")]
        [StringLength(100)]
        public string? Street { get; set; }

        [Required(ErrorMessage = "Numer domu jest wymagany")]
        [StringLength(10)]
        public string? HouseNumber { get; set; }

        [StringLength(10)]
        public string? ApartmentNumber { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany")]
        [StringLength(10)]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane")]
        [StringLength(100)]
        public string? City { get; set; }

        [Required(ErrorMessage = "Kraj jest wymagany")]
        [StringLength(100)]
        public string? Country { get; set; }

        // Navigation properties - mog¹ byæ null
        public virtual BillInfo? BillInfo { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public virtual ICollection<InvoiceLine> InvoiceLines { get; set; } = new List<InvoiceLine>();
    }
}