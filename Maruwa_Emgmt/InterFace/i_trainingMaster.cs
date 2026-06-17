using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.InterFace
{
    public interface i_trainingMaster
    {
        Task<List<trainingMaster>> GetTrainingListAsync();
        Task<trainingMaster> GetTrainingByIdAsync(string code);
        Task<bool> SaveTrainingAsync(trainingMaster model);
        Task<bool> UpdateTrainingAsync(trainingMaster model);

        Task<bool> DeleteTrainingAsync(string code);

        Task<string> GetNextTrainingCodeAsync(); // NEW → For auto code generation
    }
}
