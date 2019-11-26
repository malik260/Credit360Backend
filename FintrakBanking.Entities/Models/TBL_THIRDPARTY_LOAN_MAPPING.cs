using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_THIRDPARTY_LOAN_MAPPING")]
    public partial class TBL_THIRDPARTY_LOAN_MAPPING
    {
        [Key]
        public int THIRDPARTYLOANMAPPINGID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public string FACILITYMAPPINGID { get; set; }
        public string BOOKINGCODE { get; set; }

    }
}
