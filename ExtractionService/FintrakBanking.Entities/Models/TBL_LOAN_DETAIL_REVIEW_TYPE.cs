using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_DETAIL_REVIEW_TYPE")]
    public partial class TBL_LOAN_DETAIL_REVIEW_TYPE
    {
        [Key]
        public int LOANDETAILREVIEWTYPEID { get; set; }
        public string LOANDETAILREVIEWTYPENAME { get; set; }
    }
}
