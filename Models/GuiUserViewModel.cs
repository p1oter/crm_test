using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Models
{
    [Authorize(Policy = "CanManageClients")]
    public class GuiUserViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Login { get; set; } = null!;

        [Display(Name = "Hasło")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }  // required only on Create

        [Display(Name = "Powtórz hasło")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasła muszą być takie same")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Pracownik")]
        public ulong EmployeeId { get; set; }

        [Display(Name = "Rola")]
        public ulong RoleId { get; set; }
    }
}