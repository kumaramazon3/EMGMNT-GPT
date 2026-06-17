using Maruwa_Emgmt.DBcontex;
using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.BAL
{
    public class bal_empmaster
    {
        private readonly i_empmaster _empMaster;
        public bal_empmaster(i_empmaster empMaster)
        {
            _empMaster = empMaster;
        }

        public empmaster ValidateLoginInfo(string empcode, string pwd)
        {
            // Call BAL/DAL to validate login
            empmaster obj = _empMaster.ValidateLoginInfo(empcode, pwd);
            return obj;
        }
    }
}
