using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Maruwa_Emgmt.Models.TM
{
    [Table("trainingList", Schema = "dbo")]
    public class trainingList
    {
        [Key]
        public int Sno { get; set; }
        public string? code { get; set; }
        public string? titleName { get; set; }
        public string? programme { get; set; }
        public string? deptCode { get; set; }
        public string? deptName { get; set; }
        public string? sectionCode { get; set; }
        public string? sectionName { get; set; }
        public string? Method { get; set; }
        public string? Type { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
        public List<empMaster> Trainers { get; set; } = new();
    }
    //public class TrainerDto
    //{
    //    public string? empcode { get; set; }
    //    public string? empName { get; set; }
    //}

}
