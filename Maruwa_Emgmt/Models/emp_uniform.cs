using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models
{
    [Table("emp_uniform", Schema = "dbo")]
    public class emp_uniform
    {
        public string empcode { get; set; }
        public string? trousers { get; set; }
        public string? shirt { get; set; }
        public string shoe { get; set; }
        public string? tno { get; set; }
        public string? shino { get; set; }
        public string? shono { get; set; }
        public string? jackno { get; set; }
        public string? capno { get; set; }
        public DateTime? dategiven { get; set; }
        public string? pantsize { get; set; }
        public string? pantsizeqty { get; set; }
        public string? jackqty { get; set; }
        public string? capqty { get; set; }
        public string? safetybeltsize { get; set; }
        public string? safetybeltqrt { get; set; }
        public string? earmuffqty { get; set; }
        public string? helmetqty { get; set; }
        public string? apronqty { get; set; }
    }
}
