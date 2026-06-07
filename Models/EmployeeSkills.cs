using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Models
{
    [Table("EMPLOYEE_SKILLS")]
    public class EmployeeSkills
    {
        [Key]
        [Column("EMPLOYEE_ID")]
        public ulong EmployeeId { get; set; }

        [Key]
        [Column("SERVICE_GROUP_ID")]
        public ulong ServiceGroupId { get; set; }

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee? Employee { get; set; }

        [ForeignKey("ServiceGroupId")]
        public virtual ServiceGroup? ServiceGroup { get; set; }
    }
}