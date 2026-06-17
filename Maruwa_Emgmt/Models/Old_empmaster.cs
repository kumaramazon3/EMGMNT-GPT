using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("tblempmaster", Schema = "dbo")]
    public class Old_empmaster
    {
        [Key]
        public string empcode { get; set; }
        public string? empname { get; set; }
        public string? pwd { get; set; }
        public string emp_group { get; set; }
        public string? departmentcode { get; set; }
        public string? sectioncode { get; set; }
        public string? designation { get; set; }
        public DateTime? dateofjoin { get; set; }
        public DateTime? dateofservice { get; set; }// Check with HR-Department (Resignation Date or what?)
        public string? category { get; set; }
        public string? emptype { get; set; }
        public string? contract { get; set; }
        public byte[]? img { get; set; }
        public bool? stayinhostel { get; set; }
        public string? hostelname { get; set; }
        public bool? transportneeded { get; set; }
        public string? route { get; set; }
        public string? area { get; set; }
        public string? foreignemp { get; set; }
        public string? passportno { get; set; }
        public string? icno { get; set; }
        public string? newicno { get; set; }
        public string? bank { get; set; }
        public string? accountno { get; set; }
        public string? epf { get; set; }
        public string? sosco { get; set; }
        public string? taxcode { get; set; }
        public string? taxno { get; set; }
        public DateTime? dateofbirth { get; set; }
        public string? nationality { get; set; }
        public string? sex { get; set; }
        public string? bloodgroup { get; set; }
        public string? religion { get; set; }
        public string? race { get; set; }
        public string? maritalstatus { get; set; }
        public int? noofchildren { get; set; }
        public string? address1 { get; set; }
        public string? address2 { get; set; }
        public string? address3 { get; set; }
        public string? postalcode { get; set; }
        public string? pphone { get; set; }
        public string? php { get; set; }
        public string? email { get; set; }
        public string? edulevel { get; set; }
        public string? others { get; set; }
        public string? areaname { get; set; }
        public string? workingexp { get; set; }
        public int? hppass { get; set; }
        public int? carpass { get; set; }
        public int? computerpass { get; set; }
        public int? remotelogin { get; set; }
        public int? emptransferring { get; set; }
        public string? EmergencyContactPerson { get; set; }
        public string? EmergencyAddress { get; set; }
        public string? trousers { get; set; }
        public string? shoe { get; set; }
        public string? shirt { get; set; }
        public string? tno { get; set; }
        public string? shino { get; set; }
        public string? jackno { get; set; }
        public string? capno { get; set; }
        public string? resigned { get; set; }
        public DateTime? createdtime { get; set; }
        public string? modifiedby { get; set; }
        public bool? isactive { get; set; }

        #region Commented Code
        //public DateTime? dategiven { get; set; }
        //public string? positionid { get; set; }
        //public string? positionname { get; set; }
        //public string? Leavelevel { get; set; }
        //public DateTime? dateoftermination { get; set; }
        //public string? ldepartmentcode { get; set; }
        //public string? shift { get; set; }
        //public string? ot { get; set; }
        //public string? IsOperator { get; set; }
        //public string? RecNo { get; set; }
        //public string? empid { get; set; }
        //public DateTime? CreatedDatetime { get; set; }
        //public string? LoginBy { get; set; }
        //public DateTime? LastUpdateTime { get; set; }
        //public bool isActive { get; set; }
        #endregion
    }
}
