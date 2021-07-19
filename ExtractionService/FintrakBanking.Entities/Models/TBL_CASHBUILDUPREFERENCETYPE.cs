using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CASHBUILDUPREFERENCETYPE")]
    public partial class TBL_CASHBUILDUPREFERENCETYPE
    {
        [Key]
        public int CASHBUILDUPREFERENCETYPEID { get; set; }
        public string NAME { get; set; }
    }
}
