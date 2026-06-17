using Maruwa_Emgmt.DAL;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using System.Data;

namespace Maruwa_Emgmt.BAL
{
    public class bal_tblempmaster
    {
        private readonly i_tblempmaster _tblempMaster;
        public bal_tblempmaster(i_tblempmaster tblempMaster)
        {
            _tblempMaster = tblempMaster;
        }

        public empMaster ValidateLoginInfo(string empcode, string pwd)
        {
            empMaster obj = _tblempMaster.ValidateLoginInfo(empcode, pwd);
            return obj;
        }

        //public IEnumerable<tblempmaster> GetAllEmployeeData_Old()
        //{
        //    IEnumerable<tblempmaster> obj = _tblempMaster.GetAllEmployeeData_Old();
        //    return obj;
        //}

        public IEnumerable<empMaster> GetAllEmployeeData()
        {
            IEnumerable<empMaster> obj = _tblempMaster.GetAllEmployeeData();
            return obj;
        }
        public async Task<DataSet> GetAllEmployeeList()
        {
            return await _tblempMaster.GetAllEmployeeList();
        }

        public string GetLastEmpCode()
        {
            return _tblempMaster.GetLastEmpCode();
        }
        public List<tblhousenumber> GetHouseNumbersByHostel(string housecode)
        {
            return _tblempMaster.GetHouseNumbersByHostel(housecode);
        }

        //public async Task<string> InsertEmployeeAsync(tblempmaster model,IFormFile img,IFormFile icfile,IFormFile passportfile,IFormFile professionaldoc,IFormFile resume,IFormFile academicCertificate,IFormFile otherCertificates)
        //{
        //    return await _tblempMaster.InsertEmployeeAsync(model,img,icfile,passportfile,professionaldoc,resume,academicCertificate,otherCertificates);
        //}
        //public async Task updateEmployeeAsync(tblempmaster model, IFormFile img, IFormFile icfile, IFormFile passportfile, IFormFile professionaldoc, IFormFile resume, IFormFile academicCertificate, IFormFile otherCertificates)
        //{
        //    await _tblempMaster.UpdateEmployeeAsync(model, img, icfile, passportfile, professionaldoc, resume, academicCertificate, otherCertificates);
        //}
        //public async Task<tblempmaster?> GetByEmpCodeAsync_Old(string empcode)
        //{
        //    return await _tblempMaster.GetByEmpCodeAsync_Old(empcode);
        //}

        //public async Task<string> SaveEmployeeAsync(empMaster emp,uniformInfo uniform,accessoriesInfo accessories)
        //{
        //    return await _tblempMaster.SaveEmployeeAsync(emp, uniform, accessories);
        //}
        public async Task<string> SaveEmployeeAsync(EmployeeSaveVM model, IFormFile img, IFormFile icFilePath, IFormFile passportfileurl, IFormFile professionaldocurl, IFormFile resumeDocURL, IFormFile academicCertificateUrl, IFormFile otherCertificateUrl)
        {
            return await _tblempMaster.SaveEmployeeAsync(model, img, icFilePath, passportfileurl, professionaldocurl, resumeDocURL, academicCertificateUrl, otherCertificateUrl);
        }

        public async Task<EmployeeSaveVM?> GetByEmpCodeAsync(string empcode)
        {
            return await _tblempMaster.GetByEmpCodeAsync(empcode);
        }

    }
}
