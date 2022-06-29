using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_WFCONTEXT_VALUE_TYPE")]
    public class TBL_WFCONTEXT_VALUE_TYPE
    {
        [Key]
        public int VALUETYPEID { get; set; }
        public string VALUETYPENAME { get; set; }
        public bool INUSE { get; set; }

    }
}
