using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.SkillMatrix;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.InterFace.SkillMatrix
{
    public interface i_SkillMatrix
    {
        //Task<bool> InsertSkillMatrixAsync(skillMatrixDocumentData model);
        Task<bool> InsertSkillMatrixListAsync(List<Insert_skillMatrix> models);
        Task<bool> UpdateLogTableStatusAsync();// Move Master Data to Log Table.

        //Task DeleteAllAsync();

        Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, DateTime? logDate);
        //Task SaveSkillMatrixAsync_OLD(List<master_Skill_Matrix> skillMatrix);


        Task<List<new_skillMatrixDocumentData>> new_GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate);// Not in Use
        Task<List<SelectskillMatrixDocumentData>> GetSkillMatrixAsync(string departmentCode, string subDepartmentCode, string sectionCode, string aliasName, string? logDate);// Not in Use
        //Task<List<new_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode);
        Task<List<OriginalReport_skillMatrixDocumentData>> GetSkillMatrixReportAsync(string departmentCode, string subDepartmentCode, string sectionCode, string Reporttype, string aliasName);
        Task new_SaveSkillMatrixAsync(List<new_skillMatrixDocumentData> skillMatrix);

        List<skillMatrixlog_Years> GetLogYears();
        List<SectionCodeModel> GetAllSectionList();

        Task<List<skillMatrixReportList>> GetSkillMatrixReportListAsync(string departmentCode, string sectionCode);

        Task<List<skillMatrixReportByDocNo>> GetSkillMatrixReportByDocNo(string DocNo, string Dept, string secCode);

    }
}

