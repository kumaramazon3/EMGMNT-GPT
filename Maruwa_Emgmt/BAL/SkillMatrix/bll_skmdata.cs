using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.SkillMatrix;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.SkillMatrix;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL.SkillMatrix
{
    public class bll_skmdata
    {
        private readonly i_SkillMatrix _iskm;
        public bll_skmdata(i_SkillMatrix iskm)
        {
            _iskm = iskm;
        }
        //public async Task SaveSkillMatrixAsync(skillMatrixDocumentData model)
        //{
        //    await _iskm.InsertSkillMatrixAsync(model);
        //}
        public async Task SaveSkillMatrixListAsync(List<Insert_skillMatrix> models)
        {
            await _iskm.InsertSkillMatrixListAsync(models);
        }
        public async Task SaveSkillMatrixListAsync()
        {
            await _iskm.UpdateLogTableStatusAsync();
        }

        //public async Task DeleteAllSkillMatrixAsync()
        //{
        //    await _iskm.DeleteAllAsync();
        //}

        public async Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, DateTime? logDate)
        {
            return await _iskm.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, logDate);
        }
        
        //public async Task SaveSkillMatrixAsync(List<master_Skill_Matrix> skillMatrix, string empCode)
        //{
        //    if (skillMatrix == null || !skillMatrix.Any())
        //        throw new ArgumentException("No data provided to save.");

        //    foreach (var item in skillMatrix)
        //    {
        //        item.modifyedBy = empCode;
        //        item.modifyedDateTime = DateTime.Now;
        //    }

        //    // Call DAL to save all rows in a transaction
        //    await _iskm.SaveSkillMatrixAsync_OLD(skillMatrix);
        //}

        public async Task<List<new_skillMatrixDocumentData>> new_GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode,string aliasName, string? logDate)
        {
            try
            {
                return await _iskm.new_GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, aliasName, logDate);
            }
            catch (Exception ex)
            {
                throw; // propagate to controller
            }
        }
        public async Task<List<SelectskillMatrixDocumentData>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate)
        {
            try
            {
                return await _iskm.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, aliasName, logDate);
            }
            catch (Exception ex)
            {
                throw; // propagate to controller
            }
        }

        public async Task new_SaveSkillMatrixAsync(List<new_skillMatrixDocumentData> skillMatrix, string empCode)
        {
            if (skillMatrix == null || !skillMatrix.Any())
                throw new ArgumentException("No data provided to save.");

            // Set modified info for all rows
            foreach (var item in skillMatrix)
            {
                item.position ??= "";
                item.modifyedBy = empCode;
                item.modifyedDateTime = DateTime.Now;
            }

            // Call DAL to save
            await _iskm.new_SaveSkillMatrixAsync(skillMatrix);
        }

        public List<skillMatrixlog_Years> GetLogYears()
        {
            return _iskm.GetLogYears();
        }

        public List<SectionCodeModel> GetAllSectionList()
        {
            return _iskm.GetAllSectionList();
        }

        public async Task<List<skillMatrixReportList>> GetSkillMatrixReportListAsync(string departmentCode, string sectionCode)
        {
            try
            {
                return await _iskm.GetSkillMatrixReportListAsync(departmentCode, sectionCode);
            }
            catch (Exception ex)
            {
                throw; // propagate to controller
            }
        }


        public async Task<List<skillMatrixReportByDocNo>> GetSkillMatrixReportByDocNo(string DocNo, string Dept, string secCode)
        {
            try
            {
                return await _iskm.GetSkillMatrixReportByDocNo(DocNo, Dept, secCode);
            }
            catch (Exception ex)
            {
                throw; // propagate to controller
            }

        }


        #region Report
        public async Task<List<OriginalReport_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode,string Reporttype,string aliasName)
        {
            try
            {
                return await _iskm.GetSkillMatrixReportAsync(departmentCode, subDepartmentCode, sectionCode, Reporttype, aliasName);
            }
            catch (Exception ex)
            {
                throw; // propagate to controller
            }
        }

        internal async Task<bool> UpdateLogTableStatusAsync()
        {
            return await _iskm.UpdateLogTableStatusAsync();
        }


        #endregion
    }
}
