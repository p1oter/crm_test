using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    [Table("GUI_USER")]
    public class GuiUser
    {
        // U¿ywamy LOGIN jako klucz (unique, not null w tabeli)
        [Key]
        [Column("LOGIN")]
        public string Login { get; set; } = null!;

        [Required]
        [Column("PASSWORD_HASH")]
        public string PasswordHash { get; set; } = null!;

        // Kolumny istniej¹ce w tabeli; nie musisz ich u¿ywaæ od razu
        [Column("EMPLOYEE_ID")]
        public ulong EmployeeId { get; set; }

        [Column("ROLE_ID")]
        public ulong RoleId { get; set; }
    }
}