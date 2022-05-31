using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WORKFLOW_CONTEXT")]
    public class TBL_WORKFLOW_CONTEXT
    {
        [Key]
        public int CONTEXTID { get; set; }
        public string CONTEXTNAME { get; set; }
        public int POSITION { get; set; }
    }
}
