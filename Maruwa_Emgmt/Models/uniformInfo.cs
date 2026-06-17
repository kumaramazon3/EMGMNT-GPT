using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class uniformInfo
    {
        [Key]
        public int id { get; set; }
        public string? empcode { get; set; }
        public string? jacketSize { get; set; }
        public string? jacketqty { get; set; }
        public string? pantSize { get; set; }
        public string? pantqty { get; set; }
        public string? tShirtSize { get; set; }
        public string? tShirtqty { get; set; }
        public string? safetyShoes { get; set; }
        public string? safetyShoesqty { get; set; }
        public string? capColor { get; set; }
        public string? capColorqty { get; set; }
        public string? safetyBeltSize { get; set; }
        public string? safetyBeltqty { get; set; }
        public string? earMuffqty { get; set; }
        public string? safetyHelmetqty { get; set; }
        public string? apronQty { get; set; }
        public DateTime? dateOfIssue { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
