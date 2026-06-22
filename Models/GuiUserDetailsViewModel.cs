namespace CRM.Models
{
    public class GuiUserDetailsViewModel
    {
        public string Login { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public ulong EmployeeId { get; set; }
        public ulong RoleId { get; set; }

        // Czytelne nazwy do pokazania w widoku (mogą być null jeśli brak powiązania)
        public string? EmployeeName { get; set; }
        public string? RoleName { get; set; }
    }
}