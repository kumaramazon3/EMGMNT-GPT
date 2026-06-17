using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Maruwa_Emgmt.Models.Leave
{
    public class leaveform
    {
        [Key]
        public int appno { get; set; }
        public string empCode { get; set; }
        public string? empName { get; set; }
        public DateTime? applicationdate { get; set; }
        public double? days { get; set; }
        public double? workfor { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public string? leavetype { get; set; }
        public string? reason { get; set; }
        public string? leavetime { get; set; }
        public string? carryfwd { get; set; }
        public double? nocf { get; set; }
        public string? Status { get; set; }
        public string? statusreason { get; set; }
        public string? designation { get; set; }
        public string? department { get; set; }
        public string? Sectioncode { get; set; }
    }

    public class leaveformHistory
    {
        [Key]
        public double Uid { get; set; }
        public int? appno { get; set; }
        public DateTime? applicationdate { get; set; }
        public string? EmpCode { get; set; }
        public double? workfor { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public string? leavetype { get; set; }
        public string? reason { get; set; }
        public string? status { get; set; }

        public string? statusreason { get; set; }
        public string? leavetime { get; set; }
        public string? status1 { get; set; }
        public string? userid { get; set; }
        public double? grantedleave { get; set; }
        public string? designation { get; set; }
        public string? department { get; set; }
        public string? japanhead { get; set; }
        public string? Sectioncode { get; set; }
        public DateTime? grfromdate { get; set; }
        public DateTime? grtodate { get; set; }
        public string? desig { get; set; }
        public DateTime? approveddate { get; set; }
        public string? backdate { get; set; }
        public string? approvedby { get; set; }
        public string? leavetype1 { get; set; }
        public string? carryfwd { get; set; }
        public double? nocf { get; set; }
        public double? days { get; set; }
        public int? groupid { get; set; }
        public string? oldrecord { get; set; }
        public double? nxtyearutilised { get; set; }
        public string? nxtyearname { get; set; }
        public string? rejectedby { get; set; }
        public DateTime? rejecteddate { get; set; }
        public string? createdby { get; set; }
        public DateTime? createdtime { get; set; }
        public string? modifiedby { get; set; }
        public DateTime? modifiedtime { get; set; }
        public string? pic1 { get; set; }
        public string? pic11 { get; set; }
        public string? pic2 { get; set; }
        public string? pic22 { get; set; }
        public string? AnnualBal { get; set; }
        public string? MedicalBal { get; set; }
        public string? SupportingDocument { get; set; }

    }

    public class LeaveApprovalModel
    {
        public string AppNo { get; set; }

        [JsonPropertyName("LeaveempCode")]
        public string LeaveempCode { get; set; } // Who's empCode Approving
        public string Status { get; set; } // Approve / Reject
        public string WorkFor { get; set; } // Number of Days Leaves
        public string Nocf { get; set; } // Number of days Leaves Carry Forward
        public string Granted { get; set; } // (WorkFor + nocf)
        public string LeavetypeID { get; set; } // Annual Leave: AL
        public string carryfwd { get; set; } // Y / N
        public string Remarks { get; set; } // Login Person--> Remark
        public string Modifiedby { get; set; } // Login By
    }
    public class LeaveApprovalUpdate
    {
        public string AppNo { get; set; }
        public string EmpCode { get; set; }
        public string Status { get; set; } // Approved / Rejected
        public string? Remarks { get; set; }
        public string modifiedby { get; set; }
        public DateTime modifiedtime { get; set; }
    }

    public class LeaveApplicationSubmitModel
    {
        public string EmpCode { get; set; }
        public double days { get; set; }
        public double workfor { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; } 
        public string leavetype { get; set; }
        public string leavetype1 { get; set; }
        public string reason { get; set; }
        public string leavetime { get; set; }
        public string backdate { get; set; }// Y / N
        public double AnnualBal { get; set; }
        public double MedicalBal { get; set; }
        public string pic1 { get; set; }
        public string pic11 { get; set; }
        public string pic2 { get; set; }
        public string pic22 { get; set; }
        public string createdby { get; set; }
    }
}
