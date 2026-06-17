using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.master
{
    [Table("master_LeaveType", Schema = "dbo")]
    public class master_LeaveType
    {
        [Key]
        public int Uid { get; set; }
        public string? LeaveType { get; set; }
        public bool? IsPaid { get; set; }
        public float? AllocatedDays1 { get; set; }
        public float? AllocatedDays2 { get; set; }
        public float? AllocatedDays3 { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool? DeleteStatus { get; set; }
        public string? LTCode { get; set; }
    }

    [Table("master_LeaveTiming", Schema = "dbo")]
    public class LeaveTiming
    {
        [Key]
        public long uid { get; set; }
        public TimeSpan FromTime { get; set; }  // maps to SQL TIME(7)
        public TimeSpan ToTime { get; set; }    // maps to SQL TIME(7)
    }
    public class LeaveTimingDto
    {
        public long uid { get; set; }   // formatted string
        public string FromTime { get; set; }   // formatted string
        public string ToTime { get; set; }     // formatted string
    }
    public class inChargeEmpMasterDto
    {
        public string empCode{ get; set; }
        public string empName { get; set; }
    }


    }
