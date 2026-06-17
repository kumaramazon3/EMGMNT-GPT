using Maruwa_Emgmt.DAL;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.master;
using Maruwa_Emgmt.Models.TM;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace Maruwa_Emgmt.BAL
{
    public class bal_tbldropdownData
    {
        private readonly i_tbldropdownData _tbldropdownData;
        public bal_tbldropdownData(i_tbldropdownData tbldropdownData)
        {
            _tbldropdownData = tbldropdownData;
        }

        public IEnumerable<tbldropdownData> GetAllEmployeeData_Old()
        {
            IEnumerable<tbldropdownData> obj = _tbldropdownData.GetAllDropdownData_Old();
            return obj;
        }
        public async Task<IEnumerable<tbldropdownData>> GetAllEmployeeDataAsync()
        {
            return await _tbldropdownData.GetAllDropdownDataAsync();
        }

        public IEnumerable<tblsections> GetSectionsByDepartment(string departmentCode)
        {
            // Call the DAL method to fetch sections for the given department code
            return _tbldropdownData.GetSectionsByDepartment(departmentCode);
        }

        public IEnumerable<tblsections> GetSectionsBySubDept(string SubDeptCode)
        {
            return _tbldropdownData.GetSectionsBySubDept(SubDeptCode);
        }

        public IEnumerable<master_SubDepartment> GetSubDepartmentByDepartment(string departmentCode)
        {
            return _tbldropdownData.GetSubDepartmentByDepartment(departmentCode);
        }

        public List<object> GetLicenseTypes()
        {
            return _tbldropdownData.GetLicenseTypes();
        }
        public async Task<IEnumerable<master_EmployeeType>> Getmaster_EmployeeType()
        {
            return await _tbldropdownData.Getmaster_EmployeeType();
        }

        public async Task<List<tbldropdownData>> GetAlldropdownlistAsync()
        {
            return await _tbldropdownData.GetAlldropdownlistAsync();
        }
        public async Task<DataSet> GetAllMasterDataAsync()
        {
            return await _tbldropdownData.GetAllMasterDataAsync();
        }


        #region Master Data
        public async Task<IEnumerable<master_Nationality>> Getmaster_Nationality()
        {
            return await _tbldropdownData.Getmaster_Nationality();
        }
        public async Task<IEnumerable<master_BloodGroup>> Getmaster_BloodGroup()
        {
            return await _tbldropdownData.Getmaster_BloodGroup();
        }
        public async Task<IEnumerable<master_Bank>> Getmaster_Bank()
        {
            return await _tbldropdownData.Getmaster_Bank();
        }
        public async Task<IEnumerable<master_Education>> Getmaster_Education()
        {
            return await _tbldropdownData.Getmaster_Education();
        }
        public async Task<IEnumerable<master_Hostal>> Getmaster_Hostal()
        {
            return await _tbldropdownData.Getmaster_Hostal();
        }
        public async Task<IEnumerable<master_Language>> Getmaster_Language()
        {
            return await _tbldropdownData.Getmaster_Language();
        }

        public async Task<IEnumerable<master_Department>> Getmaster_Department()
        {
            return await _tbldropdownData.Getmaster_Department();
        }
        public async Task<IEnumerable<master_Route>> Getmaster_Route()
        {
            return await _tbldropdownData.Getmaster_Route();
        }
        public async Task<IEnumerable<master_Religion>> Getmaster_Religion()
        {
            return await _tbldropdownData.Getmaster_Religion();
        }
        public async Task<IEnumerable<master_Race>> Getmaster_Race()
        {
            return await _tbldropdownData.Getmaster_Race();
        }
        public async Task<IEnumerable<Models.master_Designation>> Getmaster_Designation()
        {
            return await _tbldropdownData.Getmaster_Designation();
        }
        public IEnumerable<master_SubDepartment> GetSubDepartmentsByDepartment(string departmentCode)
        {
            return _tbldropdownData.GetSubDepartmentsByDepartment(departmentCode);
        }
        public IEnumerable<master_DepartmentSection> GetSubDeptSections(string SubDeptCode)
        {
            return _tbldropdownData.GetSubDeptSections(SubDeptCode);
        }
        public IEnumerable<master_DepartmentSection> GetSubDeptSectionsFromSkillMatrix(string SubDeptCode)
        {
            return _tbldropdownData.GetSubDeptSectionsFromSkillMatrix(SubDeptCode);
        }

        public async Task<IEnumerable<master_Department>> Getmaster_TrainingTitle()
        {
            return await _tbldropdownData.Getmaster_Department();
        }
        #endregion

        public async Task<List<trainingMaster>> GetTrainingMasterlistAsync()
        {
            return await _tbldropdownData.GetTrainingMasterlistAsync();
        }

        public async Task<List<master_LeaveType>> GetLeaveTypeAsync()
        {
            return await _tbldropdownData.GetLeaveTypeAsync();
        }
        public async Task<List<LeaveTimingDto>> GetLeaveTimingsAsync()
        {
            return await _tbldropdownData.GetLeaveTimingsAsync();
        }
        public async Task<List<inChargeEmpMasterDto>> GetallEmployesAsync()
        {
            return await _tbldropdownData.GetallEmployesAsync();
        }
    }
}
