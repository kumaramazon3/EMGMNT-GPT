using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.master;
using Maruwa_Emgmt.Models.TM;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace Maruwa_Emgmt.InterFace
{
    public interface i_tbldropdownData
    {
        IEnumerable<tbldropdownData> GetAllDropdownData_Old();
        Task<IEnumerable<tbldropdownData>> GetAllDropdownDataAsync();

        IEnumerable<tblsections> GetSectionsByDepartment(string departmentCode);
        IEnumerable<tblsections> GetSectionsBySubDept(string SubDeptCode);
        IEnumerable<master_SubDepartment> GetSubDepartmentByDepartment(string departmentCode);
        List<object> GetLicenseTypes();
        Task<List<tbldropdownData>> GetAlldropdownlistAsync();
        Task<List<master_Nationality>> Getmaster_Nationality();
        Task<List<master_BloodGroup>> Getmaster_BloodGroup();
        Task<List<master_Bank>> Getmaster_Bank();
        Task<List<master_Education>> Getmaster_Education();
        Task<List<master_Hostal>> Getmaster_Hostal();
        Task<List<master_Language>> Getmaster_Language();
        Task<List<master_Race>> Getmaster_Race();
        Task<List<master_Religion>> Getmaster_Religion();
        Task<List<master_Route>> Getmaster_Route();
        Task<List<master_Department>> Getmaster_Department();
        Task<List<Models.master_Designation>> Getmaster_Designation();
        Task<IEnumerable<master_EmployeeType>> Getmaster_EmployeeType();
        IEnumerable<master_SubDepartment> GetSubDepartmentsByDepartment(string departmentCode);
        IEnumerable<master_DepartmentSection> GetSubDeptSections(string SubDeptCode);
        IEnumerable<master_DepartmentSection> GetSubDeptSectionsFromSkillMatrix(string SubDeptCode);
        Task<DataSet> GetAllMasterDataAsync();
        Task<List<trainingMaster>> GetTrainingMasterlistAsync();
        Task<List<master_LeaveType>> GetLeaveTypeAsync();
        Task<List<LeaveTimingDto>> GetLeaveTimingsAsync();
        Task<List<inChargeEmpMasterDto>> GetallEmployesAsync();
    }
}
