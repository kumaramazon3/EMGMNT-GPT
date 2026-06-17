using Maruwa_Emgmt.DAL.master;
using Maruwa_Emgmt.InterFace.master;
using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.BAL.master
{
    public class bll_Designation
    {
        private readonly i_Designation _da;

        public bll_Designation(i_Designation da)
        {
            _da = da;
        }

        public async Task<List<master_Designation>> GetAllDesignationAsync()
        {
            return await _da.GetAllDesignationAsync();
        }

    }
}
