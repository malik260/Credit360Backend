using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_FACILITY_MOD_DETL_FEE")]
    public partial class TBL_FACILITY_MOD_DETL_FEE
    {
        [Key]
        public int FACILITYMODDETAILFEEID { get; set; }
        public int FACILITYMODIFICATIONID { get; set; }
        public int LOANCHARGEFEEID { get; set; }
        public int CHARGEFEEID { get; set; }
        public decimal DEFAULT_FEERATEVALUE { get; set; }
        public decimal RECOMMENDED_FEERATEVALUE { get; set; }
    }
}
