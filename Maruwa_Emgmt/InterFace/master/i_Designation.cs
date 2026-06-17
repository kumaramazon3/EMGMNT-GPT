using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.InterFace.master
{
    public interface i_Designation
    {
        Task<List<master_Designation>> GetAllDesignationAsync();
    }
}
