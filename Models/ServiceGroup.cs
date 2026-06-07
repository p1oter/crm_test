namespace CRM.Models
{
    public class ServiceGroup
    {
        public ulong Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
        public virtual ICollection<EmployeeSkills> EmployeeSkills { get; set; } = new List<EmployeeSkills>();
    }
}