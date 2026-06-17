using Maruwa_Emgmt.DAL.Training;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL.Training
{
    public class bll_trainingList
    {
        private readonly i_trainingList _traininglist;
        public bll_trainingList(i_trainingList traininglist)
        {
            _traininglist = traininglist;
        }

        public async Task<trainingList?> GettraininglistByTitleAsync(string titleName, string Type)
        {
            return await _traininglist.GettraininglistByTitleAsync(titleName,Type);
        }
        public async Task<bool> InsertOrUpdateEmpTrainingAsync(empTraining model)
        {
            try
            {
                return await _traininglist.InsertOrUpdateEmpTrainingAsync(model);
            }
            catch (Exception)
            {
                throw; // will be caught by controller
            }
        }


    }
}
