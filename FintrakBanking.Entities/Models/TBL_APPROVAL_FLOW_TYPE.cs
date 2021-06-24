using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APPROVAL_FLOW_TYPE")]
    public class TBL_APPROVAL_FLOW_TYPE
    {
        [Key]
       public int APPROVALFLOWTYPEID { get; set; }
       public string FLOWTYPENAME { get; set; }
       public string DESCRIPTION { get; set; }
    }
}
