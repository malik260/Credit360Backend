using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WORKFLOW_ITEM_EXPRESSION")]
    public class TBL_WORKFLOW_ITEM_EXPRESSION
    {
        [Key]
        public int EXPRESSIONID { get; set; }
        public int CONTEXTID { get; set; }
        public int DATAITEMID { get; set; }
        public int COMPARISONID { get; set; }
        public int? IDVALUE { get; set; }
        public string TEXTVALUE { get; set; }
        public decimal NUMERICALVALUE { get; set; }
        public bool? BOOLEANVALUE { get; set; }
        public string WORKFLOWEXPRESSION { get; set; }
        public int? APPROVALBUSINESSRULEID { get; set; }
        public string CONJUCTION { get; set; }
        public string EXPRESSION { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
