namespace Maruwa_Emgmt.Models
{
    public class EmployeeSaveVM
    {
        public empMaster empMaster { get; set; }
        public uniformInfo uniformInfo { get; set; }
        public accessoriesInfo accessoriesInfo { get; set; }

        public IFormFile? professionaldocurl { get; set; }
        public IFormFile? otherCertificateUrl { get; set; }
        public IFormFile? resumeDocURL { get; set; }
        public IFormFile? academicCertificateUrl { get; set; }
        public IFormFile? icFilePath { get; set; }
        public IFormFile? passportfileurl { get; set; }
    }
}
