using Maruwa_Emgmt.Models;
using System.Data;

namespace Maruwa_Emgmt.InterFace
{
    public interface i_tblempmaster
    {
        string GetLastEmpCode();// Find the lastest empcode from the tbmempmaster
        //IEnumerable<tblempmaster> GetAllEmployeeData_Old();
        IEnumerable<empMaster> GetAllEmployeeData();
        //Task<IEnumerable<DataTable>> GetAllEmployeeDataAsync();
        Task<DataSet> GetAllEmployeeList();

        empMaster ValidateLoginInfo(string empcode, string pwd);
        List<tblhousenumber> GetHouseNumbersByHostel(string hostelId);
        //Task<string> InsertEmployeeAsync(tblempmaster model,IFormFile img,IFormFile icfile,IFormFile passportfile,IFormFile professionaldoc,IFormFile resume,IFormFile academicCertificate,IFormFile otherCertificates);
        //Task UpdateEmployeeAsync(tblempmaster model,IFormFile img,IFormFile icfile,IFormFile passportfile,IFormFile professionaldoc,IFormFile resume,IFormFile academicCertificate,IFormFile otherCertificates);
       
        //Task<tblempmaster?> GetByEmpCodeAsync_Old(string empcode);
        Task<EmployeeSaveVM?> GetByEmpCodeAsync(string empcode);

        //Task<string> SaveEmployeeAsync(empMaster emp,uniformInfo uniform,accessoriesInfo accessories);
        Task<string> SaveEmployeeAsync(EmployeeSaveVM emp, IFormFile img, IFormFile icfile, IFormFile passportfile, IFormFile professionaldoc, IFormFile resume, IFormFile academicCertificate, IFormFile otherCertificates);

    }
    public interface IFtpService
    {
        Task<string> UploadAsync(IFormFile file);
    }
    public interface IProjectLocationService
    {
        Task<string> UploadAsync(IFormFile file, string empcode, string filePrefix);
    }

}
