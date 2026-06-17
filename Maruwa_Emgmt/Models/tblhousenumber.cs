using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("tblhousenumber", Schema = "dbo")]
    public class tblhousenumber
    {
        [Key]
        public int sno { get; set; }
        public string housecode { get; set; }
        public string housenumber { get; set; }
        public DateTime createddatetime { get; set; }
        public bool isActive { get; set; }
    }
}
