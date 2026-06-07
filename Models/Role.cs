namespace CRM.Models
{
    public class Role
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public bool CanManageEmployees { get; set; }
        public bool CanManageClients { get; set; }
        public bool CanManagePrices { get; set; }
        public bool CanManageInvoices { get; set; }
        public bool CanMakeReservations { get; set; }
        public bool CanManageGuiUsers { get; set; }
        public bool CanManageServices { get; set; }

        public virtual ICollection<GuiUser> GuiUsers { get; set; } = new List<GuiUser>();
    }
}