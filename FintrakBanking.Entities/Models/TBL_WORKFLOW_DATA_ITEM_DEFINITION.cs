using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WORKFLOW_DATA_ITEM_DEFINITION")]
    public class TBL_WORKFLOW_DATA_ITEM_DEFINITION
    {
        [Key]
        public int DATAITEMID { get; set; }
        public string DATAITEMNAME { get; set; }
        public int CONTEXTID { get; set; }
        public int VALUETYPEID { get; set; }
    }
}
