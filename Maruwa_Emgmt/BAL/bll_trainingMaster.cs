using Maruwa_Emgmt.DAL;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL
{
    public class bll_trainingMaster
    {
        private readonly i_trainingMaster _dal;
        public bll_trainingMaster(i_trainingMaster dal)
        {
            _dal = dal;
        }

        public async Task<string> GetNextTrainingCodeAsync()
        {
            return await _dal.GetNextTrainingCodeAsync();
        }

        public async Task<List<trainingMaster>> GetTrainingListAsync() => await _dal.GetTrainingListAsync();

        public async Task<trainingMaster> GetTrainingByIdAsync(string code) => await _dal.GetTrainingByIdAsync(code);

        public async Task<bool> SaveTrainingAsync(trainingMaster model) => await _dal.SaveTrainingAsync(model);

        public async Task<bool> UpdateTrainingAsync(trainingMaster model)
        {
            return await _dal.UpdateTrainingAsync(model);
        }


        public async Task<bool> DeleteTrainingAsync(string code)
        {
            return await _dal.DeleteTrainingAsync(code);
        }

    }
}
