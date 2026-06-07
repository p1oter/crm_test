namespace CRM.Models
{
    public class EmployeeSkills
    {
        public ulong EmployeeId { get; set; }
        public ulong ServiceGroupId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual ServiceGroup ServiceGroup { get; set; }
    }
}