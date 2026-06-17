using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.TM
{
    [Table("ProvidedMaster", Schema = "dbo")]
    public class providedMaster
    {
        [Key]
        public string? code { get; set; }
        public string? registrationNo { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
        public string? courses { get; set; }
        public string? others { get; set; }
        public string? trainers { get; set; }
        public string? contactNo { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
