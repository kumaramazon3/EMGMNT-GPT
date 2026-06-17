using Maruwa_Emgmt.DAL.Training;
using Maruwa_Emgmt.InterFace.Training;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.BAL.Training
{
    public class bll_trainingProgram
    {
        private readonly i_trainingProgram _repo;

        public bll_trainingProgram(i_trainingProgram repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<trainingProgram>> GetAllTrainingProgramsAsync()
        {
            return await _repo.GetAlltrainingProgramAsync();
        }
        public async Task<trainingProgram?> GetTrainingByIdAsync(int uid)
        {
            return await _repo.GetTrainingByIdAsync(uid);
        }
    }
}
