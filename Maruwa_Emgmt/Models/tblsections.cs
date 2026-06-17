using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class tblsections
    {
        [Key]
        public string sectioncode { get; set; }
        public string aqiscode { get; set; }
        public string departmentcode { get; set; }
        public string sectionname { get; set; }
        public DateTime createddatetime { get; set; }
        public string createdby { get; set; }
        public bool isActive { get; set; }
    }
}
