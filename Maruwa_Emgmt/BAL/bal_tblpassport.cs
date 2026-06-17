using Maruwa_Emgmt.InterFace;
using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.BAL
{
    public class bal_tblpassport
    {
        private readonly i_tblpassport _i_tblpassport;
        public bal_tblpassport(i_tblpassport tblpassport)
        {
            _i_tblpassport = tblpassport;
        }
    }
}
