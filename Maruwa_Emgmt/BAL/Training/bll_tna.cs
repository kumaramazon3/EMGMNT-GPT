using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL.Training
{
    public class bll_tna
    {
        private readonly i_tna _tna;
        public bll_tna(i_tna msttna)
        {
            _tna = msttna;
        }

        public async Task<List<master_Skill_Matrix>> GetSkillMatrixAsync(string departmentCode,string subDepartmentCode,string sectionCode,DateTime? logDate)
        {
            return await _tna.GetSkillMatrixAsync(departmentCode, subDepartmentCode, sectionCode, logDate);
        }

        public async Task SaveSkillMatrixAsync(List<master_Skill_Matrix> skillMatrix,string empCode)
        {
            if (skillMatrix == null || !skillMatrix.Any())
                throw new ArgumentException("No data provided to save.");

            foreach (var item in skillMatrix)
            {
                item.modifyedBy = empCode;
                item.modifyedDateTime = DateTime.Now;
            }

            // Call DAL to save all rows in a transaction
            await _tna.SaveSkillMatrixAsync(skillMatrix);
        }
    }
}
