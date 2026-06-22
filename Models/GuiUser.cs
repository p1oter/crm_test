using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    [Table("GUI_USER")]
    public class GuiUser
    {
        // u¿ywamy LOGIN jako klucza (tabela ma UNIQUE LOGIN)
        [Key]
        [Column("LOGIN")]
        [MaxLength(100)]
        public string Login { get; set; } = null!;

        [Required]
        [Column("PASSWORD_HASH")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Column("EMPLOYEE_ID")]
        public ulong EmployeeId { get; set; }

        [Column("ROLE_ID")]
        public ulong RoleId { get; set; }
    }
}