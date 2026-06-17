using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("empmaster", Schema = "dbo")]
    public class tblempmaster
    {
        #region Div-1 (Employee Details) 09-Columns
        [Key]
        public string? empcode { get; set; }
        public string? empname { get; set; }
        public string? pwd { get; set; }
        public string? resigned { get; set; }
        public string? departmentcode { get; set; }
        public string? sectioncode { get; set; }
        public string? designation { get; set; }
        public DateTime? dateofjoin { get; set; }
        public string? emptype { get; set; }
        public string? contract { get; set; }
        public byte[]? img { get; set; }// storing in Bytes in DB
        #endregion

        #region Div-2 (Employee Particular) 20-Columns
        public string? nationality { get; set; }
        public string? foreignemp { get; set; }// Y?N
        public string? newicno { get; set; }
        public string? icfileurl { get; set; }
        public DateTime? dateofbirth { get; set; }
        public string? bloodgroup { get; set; }
        public string? passportno { get; set; }
        public string? password { get; set; }
        public string? passportfileurl { get; set; }
        public string? sex { get; set; }
        public string? email { get; set; }
        public string? pphone { get; set; }// Mobile No
        public string? religion { get; set; }
        public string? race { get; set; }
        public string? maritalstatus { get; set; }
        public string? spousename { get; set; }
        public string? isspouseworking { get; set; } // Y?N
        public string? noofchildren { get; set; }
        public string? address1 { get; set; }
        public string? sameaspermanent { get; set; }
        public string? address2 { get; set; }
        #endregion

        #region Div-3 (Hostel, Licences, Bank & Tax Details) 19-Columns
        public string? stayinhostel { get; set; }
        public string? hostelname { get; set; }
        public string? houseno { get; set; }
        //public string? typehouseNo { get; set; }
        public string? transportneeded { get; set; }
        public string? route { get; set; }
        public string? drivinglicense { get; set; }
        public string? licensetype { get; set; }
        public string? vehicleno { get; set; }
        public string? vehicleno2 { get; set; }
        public string? vehicleno3 { get; set; }
        public string? bank { get; set; }
        public string? accountno { get; set; }
        public string? epf { get; set; }
        public string? sosco { get; set; }
        public string? taxno { get; set; }
        public string? EmergencyContactPerson { get; set; }
        public string? emergencyContactNumber { get; set; }
        public string? relation { get; set; }
        public string? EmergencyAddress { get; set; }

        #endregion

        #region Div-4 (Education Details) 09-Columns
        public string? edulevel { get; set; }
        public string? languagespeaking { get; set; }
        public string? proficiencylevel { get; set; }
        public string? fieldstudy { get; set; }
        public string? professionaldocurl { get; set; }
        public string? otherqualification { get; set; }
        public string? resumeurl { get; set; }
        public string? academicCertificateurl { get; set; }
        public string? othercertificatesurl { get; set; }

        #endregion

        #region Div-5 (Uniform and PPE Details) 16-Columns
        public string? jackno { get; set; }// Jacket Size
        public string? jackqty { get; set; }
        public string? safetybeltsize { get; set; }
        public string? safetybeltqty { get; set; }
        public string? shirt { get; set; }// T-Shirt Size
        public string? tshirtqty { get; set; }
        public string? earmuffQty { get; set; }
        public string? pantsize { get; set; }
        public string? pantqty { get; set; }
        public string? helmetqty { get; set; }
        public string? shoeno { get; set; }// Safety Shoes
        public string? shoeqty { get; set; }
        public string? apronqty { get; set; }
        public string? capcolor { get; set; }
        public string? capqty { get; set; }
        public DateTime? uniformdate { get; set; }
        #endregion

        #region Div-6 (Access Columns 20)
        public string? hppass { get; set; }// Hand Phone
        public DateTime? HPissuedate { get; set; }
        public string? laptop { get; set; }// laptop
        public DateTime? laptopissuedate { get; set; }
        public string? tablet { get; set; }// laptop
        public DateTime? tabletissuedate { get; set; }
        public string? php { get; set; }// Cpmapny Phone
        public DateTime? companyphoneissuedate { get; set; }
        //public string? walkietoki { get; set; }
        public string? walkietalkie { get; set; }
        public DateTime? walkietalkieissuedate { get; set; }
        //public DateTime? walkietokiissuedate { get; set; }
        public string? internetaccess { get; set; }
        public DateTime? internetaccessissuedate { get; set; }
        public string? computerpass { get; set; }// System Acces
        public DateTime? computerPassissuedate { get; set; }
        public string? folderaccess { get; set; }
        public DateTime? folderaccessissuedate { get; set; }
        public string? windowsloginID { get; set; }
        public DateTime? windowsloginIDissuedate { get; set; }
        public string? companyemail { get; set; }
        public DateTime? companyemailissuedate { get; set; }

        #endregion

        #region Extra columns (18) we not insert data
        public string? emp_group { get; set; }
        public DateTime? dateofservice { get; set; }// Check with HR-Department (Resignation Date or what?)
        public string? category { get; set; }
        public string? area { get; set; }
        public string? icno { get; set; }
        public string? taxcode { get; set; }
        public string? address3 { get; set; }
        public string? postalcode { get; set; }
        public string? areaname { get; set; }
        public string? workingexp { get; set; }
        public string? carpass { get; set; }
        public string? remotelogin { get; set; }
        public string? emptransferring { get; set; }
        public string? trousers { get; set; }
        public string? shoe { get; set; }
        public string? tno { get; set; }
        public string? shino { get; set; }
        public string? capno { get; set; }
        #endregion

        #region Auto Bind Columns (10)
        //public string? pwd { get; set; }
        public DateTime? createdtime { get; set; }
        public string? modifiedby { get; set; }
        public string? createdby { get; set; }
        public DateTime? modifiedtime { get; set; }
        public bool? isactive { get; set; }
        //public string? resigned { get; set; }
        public string? Addemp { get; set; }
        public string? viewemp { get; set; }
        public string? editemp { get; set; }
        public string? deleteemp { get; set; }
        #endregion

        #region Adding new Columns on 24-12-2025
        public string? positionid { get; set; }
        public string? totcontract { get; set; }
        public string? Confirmed { get; set; }
        public string? extendcontract { get; set; }
        public DateTime? contracteffectivefrom { get; set; }
        public string? shift { get; set; }
        public string? ot { get; set; }
        public string? IsOperator { get; set; }
        //public int? RecNo { get; set; }
        public int? empcodei { get; set; }
        public DateTime? confirmdate { get; set; }
        public string? nextyear { get; set; }

        #endregion

    }
    public class SectionCodeModel
    {
        public string SectionCode { get; set; }
    }
}
