using Maruwa_Emgmt.Models;
using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.InterFace.Training
{
    public interface i_empTraining
    {
        Task<empMaster?> GetByEmpCodeAsync(string empcode);
    }
    public interface i_trainingProgram
    {
        Task<IEnumerable<trainingProgram>> GetAlltrainingProgramAsync();
        Task<trainingProgram?> GetTrainingByIdAsync(int uid);
    }
}
