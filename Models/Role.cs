using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    [Table("ROLE")]
    public class Role
    {
        [Key]
        [Column("ID")]
        public ulong Id { get; set; }

        [Required]
        [Column("NAME")]
        public string Name { get; set; } = null!;

        [Column("CAN_MANAGE_EMPLOYEES")]
        public bool CanManageEmployees { get; set; }

        [Column("CAN_MANAGE_CLIENTS")]
        public bool CanManageClients { get; set; }

        [Column("CAN_MANAGE_PRICES")]
        public bool CanManagePrices { get; set; }

        [Column("CAN_MANAGE_INVOICES")]
        public bool CanManageInvoices { get; set; }

        [Column("CAN_MAKE_RESERVATIONS")]
        public bool CanMakeReservations { get; set; }

        [Column("CAN_MANAGE_GUI_USERS")]
        public bool CanManageGuiUsers { get; set; }

        [Column("CAN_MANAGE_SERVICES")]
        public bool CanManageServices { get; set; }
    }
}