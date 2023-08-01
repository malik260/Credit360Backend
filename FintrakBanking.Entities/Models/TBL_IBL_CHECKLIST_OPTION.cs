using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_IBL_CHECKLIST_OPTION")]
    public class TBL_IBL_CHECKLIST_OPTION
    {
        [Key]
        public int OPTIONID { get; set; }
        public int IBLCHECKLISTID { get; set; }
        public string OPTIONNAME { get; set; }
        
    }
}