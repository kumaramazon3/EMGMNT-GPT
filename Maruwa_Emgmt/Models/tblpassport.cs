using System;
using System.ComponentModel.DataAnnotations;

namespace Maruwa_Emgmt.Models
{
    public class tblpassport
    {
        [Key]
        public int Sno { get; set; }
        public string? empcode { get; set; }
        public string? type { get; set; }
        public string? countrycode { get; set; }
        public string? passportno { get; set; }
        public string? surname { get; set; }
        public string? givenname { get; set; }
        public string? nationality { get; set; }
        public string? sex { get; set; }
        public string? dateofbirth { get; set; }
        public string? placeofbirth { get; set; }
        public string? placeofissue { get; set; }
        public string? dateofissue { get; set; }
        public string? dateofexpiry { get; set; }
        // Visa information
        public string? v_vpno { get; set; }
        public string? v_passportno { get; set; }
        public string? v_name { get; set; }
        public string? v_refno { get; set; }
        public string? v_issuedate { get; set; }
        public string? v_placeofIssue { get; set; }
        public string? v_validuntil { get; set; }
        public string? v_position { get; set; }
        public string? v_company { get; set; }
        public string? v_companydddress { get; set; }
        public DateTime? createddatetime { get; set; }
        public string? createdby { get; set; }
        public bool? isactive { get; set; }
        public DateTime? lastupdateddatetime { get; set; }
    }
}
