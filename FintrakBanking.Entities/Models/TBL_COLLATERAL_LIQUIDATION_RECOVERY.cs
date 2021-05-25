using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_LIQUIDATION_RECOVERY")]
    public partial class TBL_COLLATERAL_LIQUIDATION_RECOVERY
    {
        [Key]
        public int COLLATERALLIQUIDATIONRECOVERYID { get; set; }
        public int LOANID { get; set; }
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public string CUSTOMERID { get; set; }
        public int ACCREDITEDCONSULTANT { get; set; }
        public bool ISFULLYRECOVERED { get; set; }
        public int CREATEDBY { get; set; }
        public int LOANASSIGNID { get; set; }
        public byte[] FILEDATA { get; set; }
        public string FILENAME { get; set; }
        public string FILEEXTENSION { get; set; }
        public int? FILESIZE { get; set; }
        public string FILESIZEUNIT { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime RECEIPTDATE { get; set; }
        public decimal TOTALRECOVERYAMOUNT { get; set; }
        public decimal RECOVEREDAMOUNT { get; set; }
        public decimal? PERCENTAGECOMMISSION { get; set; }
        public decimal? OUTSTANDINGAMOUNT { get; set; }
        public string COLLATERALCODE { get; set; }
        public string COLLECTIONMODE { get; set; }
        public string LOANREFERENCE { get; set; }
    }
}
