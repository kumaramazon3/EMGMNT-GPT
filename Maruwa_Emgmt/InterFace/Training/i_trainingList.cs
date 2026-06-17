using Maruwa_Emgmt.Models.TM;

namespace Maruwa_Emgmt.InterFace.Training
{
    public interface i_trainingList
    {
        Task<trainingList?> GettraininglistByTitleAsync(string titleName, string Type);
        Task<bool> InsertOrUpdateEmpTrainingAsync(empTraining model);
    }
}
