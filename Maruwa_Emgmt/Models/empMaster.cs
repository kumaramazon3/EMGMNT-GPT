using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class empMaster
    {
        [Key]
        public int recNo { get; set; }
        public string? empcode { get; set; }
        public string? empName { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string? gender { get; set; }
        public DateTime? dateOfJoin { get; set; }
        public string? empType { get; set; }
        public string? contractperiod { get; set; }
        public string? nationality { get; set; }
        public string? isForeign { get; set; }
        public string? bloodGroup { get; set; }
        public string? icNo { get; set; }
        public string? icFilePath { get; set; }
        public string? emailID { get; set; }
        public string? mobileNo { get; set; }
        public string? religion { get; set; }
        public string? race { get; set; }
        public string? maritalStatus { get; set; }
        public string? spouseName { get; set; }
        public string? spouseWorking { get; set; }
        public string? noOfChildren { get; set; }
        public string? permanentAddress { get; set; }
        public string? currentAddress { get; set; }
        public DateTime? dateOfService { get; set; }
        public string? leaveLevel { get; set; }
        public DateTime? dateOfTerimination { get; set; }
        public string? epf { get; set; }
        public string? sosco { get; set; }
        public string? taxNo { get; set; }
        public string? workingExp { get; set; }
        public string? others { get; set; }// Other Course
        public byte[]? photo { get; set; }// Image
        public string? isResigned { get; set; }
        public string? isOperator { get; set; }
        public string? emergencyContactPerson { get; set; }
        public string? emergencyAddress { get; set; }
        public string? emergencyContact { get; set; }
        public string? relation { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string? department { get; set; }
        //public string? departmentName { get; set; }
        public string? subDepartment { get; set; }
        //public string? subDepartmentName { get; set; }
        public string? section { get; set; }
        public string? designation { get; set; }
        public string? isStayingHostel { get; set; }
        public string? hostelName { get; set; }
        public string? houseNo { get; set; }
        public string? isTransport { get; set; }
        public string? route { get; set; }
        public string? isDrivingLicense { get; set; }
        public string? License { get; set; }
        public string? vehicleNo1 { get; set; }
        public string? vehicleNo2 { get; set; }
        public string? vehicleNo3 { get; set; }
        public string? bankName { get; set; }
        public string? accountNumber { get; set; }
        public string? educationLevel { get; set; }
        public string? languageProficiency { get; set; }
        public string? proficiencyLevel { get; set; }
        public string? fieldOfStudy { get; set; }
        public string? professionalDocurl { get; set; }// URL
        public string? otherQualifications { get; set; }// URL
        public string? resumeDocURL { get; set; }// URL
        public string? academicCertificateUrl { get; set; }// URL
        public string? otherCertificateUrl { get; set; } // URL
        public string? passportfileurl { get; set; } // URL
        public string? passportNo { get; set; }
        public string? pwd { get; set; }

    }

    public class EmpDesignationSettings
    {
        public List<string> Allowed { get; set; }
    }
}
