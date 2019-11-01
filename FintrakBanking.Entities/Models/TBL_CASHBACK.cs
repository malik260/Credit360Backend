using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CASHBACK")]
    public partial class TBL_CASHBACK
    {
        [Key]
        public int CASHBACKID { get; set; }

        public string BACKGROUND { get; set; }
        public string ISSUES { get; set; }
        public string REQUEST { get; set; }
        public int OPERATIONID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }

      
    }
}
