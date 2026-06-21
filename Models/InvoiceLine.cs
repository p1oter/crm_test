using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CRM.Models
{
    public class InvoiceLine
    {
        public ulong Id { get; set; }

        // InvoiceId mo¿e byæ null podczas tworzenia nowej faktury (jeœli dopiero dodajemy nag³ówek),
        // ale po zapisie powinien byæ ustawiony - w controllerze ustawiamy InvoiceId przed SaveChanges
        public ulong? InvoiceId { get; set; }

        [Required(ErrorMessage = "Us³uga jest wymagana")]
        public ulong ServiceId { get; set; }

        [Required(ErrorMessage = "Wykonawca jest wymagany")]
        public ulong EmployeeId { get; set; }

        // DB ma CLIENT_ID NOT NULL - w controllerze ustawiamy to z Invoice.ClientId przed SaveChanges
        [Required]
        public ulong ClientId { get; set; }

        [Required(ErrorMessage = "Cena jest wymagana")]
        [Range(0.0, double.MaxValue, ErrorMessage = "Cena musi byæ >= 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }  // NIE nullowalny — u³atwia walidacjê i parsowanie

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Iloœæ musi byæ >= 1")]
        public uint Amount { get; set; }

        // navigation properties - NULLABLE and not validated to avoid ModelState errors
        [ValidateNever]
        public virtual Invoice? Invoice { get; set; }

        [ValidateNever]
        public virtual Service? Service { get; set; }

        [ValidateNever]
        public virtual Employee? Employee { get; set; }

        [ValidateNever]
        public virtual Client? Client { get; set; }
    }
}