using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CONSUMER_PROTECTION")]
    public partial class TBL_CONSUMER_PROTECTION
    {
        [Key]
        public int CONSUMERPROTECTIONID { get; set; }

        public int COMPANYID { get; set; }

        public double ANNUALINTERESTRATE { get; set; }
        public double TOTALFEES { get; set; }
        public double INSURANCE { get; set; }
        public double LOANAPR { get; set; }
        public short TERMOFLOANSINYEARS { get; set; }

        public decimal LOANAMOUNT { get; set; }
        public decimal MONTHLYPAYMENT { get; set; }
        public decimal TOTALFEESANDCHARGES { get; set; }
        public decimal ACTUALAMOUNTBORROWED { get; set; }

        public short BRANCHID { get; set; }

        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
    }
}
