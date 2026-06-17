using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.InterFace.Training
{
    public interface i_tna
    {
        Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode,string subDepartmentCode,string sectionCode,DateTime? logDate);
        Task SaveSkillMatrixAsync(List<master_Skill_Matrix> skillMatrix);
    }
}
