using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN")]
    public partial class TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
    {
        [Key]
        public int ID { get; set; }
        public string CUSTOMERID { get; set; }
        public DateTime? DATE { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string GROUPOBLIGORNAME { get; set; }
        public string REFERENCENUMBER { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string ACCOUNTOFFICERCODE { get; set; }
        public string ACCOUNTOFFICERNAME { get; set; }
        public string ALPHACODE { get; set; }
        public string PRODUCTCODE { get; set; }
        public string CURRENCYNAME { get; set; }
        public string PRODUCTNAME { get; set; }
        //public string FACILITYTYPE { get; set; }
        public string ADJFACILITYTYPE { get; set; }
        public string ADJFACILITYTYPEid { get; set; }
        public string ODSTATUS { get; set; }
        public string EXPIRINGBAND { get; set; }
        public int? EXPIRYBANDID { get; set; }
        public string CURRENCYTYPE { get; set; }
        public string CBNSECTOR { get; set; }
        public string CBNSECTORID { get; set; }
        public string CBNSECTORADJUSTED { get; set; }
        public string CBNCLASSIFICATION { get; set; }
        public string PWCCLASSIFICATION { get; set; }
        public string IFRSCLASSIFICATION { get; set; }
        public string TENOR { get; set; }
        public DateTime? MATURITYDATE { get; set; }
        //public string MATURITYDATE { get; set; }
        public string LOCATION { get; set; }
        public DateTime? BOOKINGDATE { get; set; }
        public DateTime? VALUEDATE { get; set; }
        public string MATURITYBAND { get; set; }
        public int? MATURITYBANDID { get; set; }
        public string CUSTOMERTYPE { get; set; }
        public string BRANCHNAME { get; set; }
        public string BRANCHCODE { get; set; }
        public string OBLIGORRISKRATING { get; set; }
        public DateTime? LASTCRDATE { get; set; }
        public string PRODUCTID { get; set; }
        public string EXPOSURETYPECODE { get; set; }
        public string EXPOSURETYPE { get; set; }
        public string TEAMCODE { get; set; }
        public decimal? LASTCREDITAMOUNT { get; set; }
        public decimal? PRINCIPALOUTSTANDINGBALTCY { get; set; }
        public decimal? PRINCIPALOUTSTANDINGBALLCY { get; set; }
        public decimal? LOANAMOUNYTCY { get; set; }
        public decimal? LOANAMOUNYLCY { get; set; }
        public string CARDLIMIT { get; set; }
        public string FXRATE { get; set; }
        public decimal? SHF { get; set; }
        public string INTERESTRATE { get; set; }
        //public decimal? SECTIONEDLOANLIMITDIRECTFCY { get; set; }
        //public decimal? SECTIONEDLOANLIMITDIRECTLCY { get; set; }
        //public decimal? TOTALEXPOSULCYPREVIOUSYEAR { get; set; }
        //public decimal? TOTAEXPOSURELCYPREVIOUS6MONTHS { get; set; }
        //public decimal? TOTAEXPOSURELCYPREVIOUS3MONTHS { get; set; }
        //public decimal? TOTAEXPOSURELCYPREVIOUSMONTHS { get; set; }
        //public decimal? TOTAEXPOSURELCYPREVIOUSDAY { get; set; }
        public decimal? TOTALEXPOSURE { get; set; }
        public decimal? TOTALUNSETTLEDAMOUNT { get; set; }
        public decimal? IMPAIRMENTAMOUNT { get; set; }
        public decimal? TOTALUNPAIDOBLIGATION { get; set; }
        public decimal? UNPOINTERESTAMOUNT { get; set; }
        public decimal? INTERESTRECIEVABLETCY { get; set; }
        public decimal? AMOUNTDUE { get; set; }
        public decimal? NPL { get; set; }
        public int? UNPODAYSOVERDUE { get; set; }
        public DateTime? SCHEDULEDUEDATE { get; set; }

        public string REGIONCODE { get; set; }
        public string REGIONNAME { get; set; }
        public string GROUPHEADNAME { get; set; }
        public string GROUPNAME { get; set; }
        public string GROUPCODE { get; set; }
        public string DIVISIONCODE { get; set; }
        public string DIVISIONNAME { get; set; }
        public string PHONENO { get; set; }
        public string EMAIL { get; set; }

    }
}
