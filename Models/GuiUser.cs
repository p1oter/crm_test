namespace CRM.Models
{
    public class GuiUser
    {
        public ulong EmployeeId { get; set; }
        public ulong RoleId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Role Role { get; set; }
    }
}