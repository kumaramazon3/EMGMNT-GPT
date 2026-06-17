using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class accessoriesInfo
    {
        [Key]
        public int id { get; set; }
        public string? empcode { get; set; }
        public string? cpmHandPhone { get; set; }
        public string? cpmHandphoneBrand { get; set; }// Need to display
        public string? cpmHandphoneSerialNo { get; set; }// Need to display
        public DateTime? cpmHandphoneIssuedDate { get; set; }
        public DateTime? cpmHandphoneReturnedDate { get; set; }// Need to display
        public string? cpmLaptop { get; set; }
        public string? cpmLaptopBrand { get; set; }// Need to display
        public string? cpmLaptopSerialNo { get; set; }// Need to display
        public DateTime? cpmLaptopIssuedDate { get; set; }
        public DateTime? cpmLaptopReturnedDate { get; set; }// Need to display
        public string? cpmTablet { get; set; }
        public string? cpmTabletBrand { get; set; }// Need to display
        public string? cpmTabletSerialNo { get; set; }// Need to display
        public DateTime? cpmTabletIssuedDate { get; set; }
        public DateTime? cpmTabletReturnedDate { get; set; }// Need to display
        public string? cpmSimcard { get; set; }
        public string? cpmSimcardBrand { get; set; }// Need to display
        public string? cpmSimcardSerialNo { get; set; }// Need to display
        public DateTime? cpmSimcardIssuedDate { get; set; }
        public DateTime? cpmSimcardReturnedDate { get; set; }// Need to display
        public string? cpmWalkieTalkie { get; set; }
        public string? cpmWalkieTalkieBrand { get; set; }// Need to display
        public string? cpmWalkieTalkieSerialNo { get; set; }// Need to display
        public DateTime? cpmWalkieTalkieIssuedDate { get; set; }
        public DateTime? cpmWalkieTalkieReturnedDate { get; set; }// Need to display
        public string? cpmInternetAccess { get; set; }
        public string? cpmInternetAccessBrand { get; set; }// Need to display
        public string? cpmInternetAccessSerialNo { get; set; }// Need to display
        public DateTime? cpmInternetAccessIssuedDate { get; set; }
        public DateTime? cpmInternetAccessReturnedDate { get; set; }// Need to display
        public string? windowsLoginID { get; set; }
        public DateTime? windowsLoginIDIssuedDate { get; set; }
        public string? companyEmail { get; set; }
        public DateTime? companyEmailIssuedDate { get; set; }
        public string? createdBy { get; set; }
        public DateTime? createdDate { get; set; }
        public string? modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}
