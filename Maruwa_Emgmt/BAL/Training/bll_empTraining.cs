using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL.Training
{
    public class bll_empTraining
    {
        private readonly i_empTraining _emptraining;
        public bll_empTraining(i_empTraining tblempMaster)
        {
            _emptraining = tblempMaster;
        }

        public async Task<empMaster?> GetByEmpCodeAsync(string empcode)
        {
            return await _emptraining.GetByEmpCodeAsync(empcode);
        }
    }
}
