using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_STAMP_DUTY_CONDITION")]
    public class TBL_STAMP_DUTY_CONDITION
    {
        [Key]
        public int CONDITIONID { get; set; }
        public int COLLATERALSUBTYPEID { get; set; }
        public decimal DUTIABLEVALUE { get; set; }
        public bool ISPERCENTAGE { get; set; }
        public bool USETENOR { get; set; }
        public int TENOR { get;set; }
    }
}