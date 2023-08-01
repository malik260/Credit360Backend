using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_IBL_CHECKLIST")]
    public class TBL_IBL_CHECKLIST
    {
        [Key]
        public int IBLCHECKLISTID { get; set; }
        public string CHECKLIST { get; set; }
    }
}