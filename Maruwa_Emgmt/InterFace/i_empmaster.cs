using Maruwa_Emgmt.Models;

namespace Maruwa_Emgmt.InterFace
{
    public interface i_empmaster
    {
        empmaster ValidateLoginInfo(string empcode, string pwd);
    }
}
