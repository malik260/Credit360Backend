using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WORKFLOW_AMEND")]
    public class TBL_WORKFLOW_AMEND
    {
         [Key]
         public int  WORKFLOWAMENDID { get; set; }
         public int  PRODUCTID { get; set; }
         public string  APPROVALSTAGE { get; set; }
         public string  CONDITIONCLAUSE { get; set; }
         public string  ACTION { get; set; }
         public decimal  AMOUNT { get; set; }
    }
}