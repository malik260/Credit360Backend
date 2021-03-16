using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_INSURANCE_TRACKING")]
    public class TBL_COLLATERAL_INSURANCE_TRACKING
    {
        [Key]
       public int COLLATERALINSURANCETRACKINGID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public decimal SUMINSURED { get; set; }
        public decimal PREMIUMPAID { get; set; }
        public DateTime INSURANCESTARTDATE { get; set; }
        public DateTime INSURANCEENDDATE { get; set; }
        public int INSURANCEPOLICYTYPEID { get; set; }
        public int INSURANCESTATUSID { get; set; }
        public string INSURANCECOMPANYNAME { get; set; }
        public string ISURANCECOMPANYADDRESS { get; set; }
        public string POLICYNUMBER { get; set; }
       
        public DateTime VALUATIONSTARTDATE { get; set; }
        public DateTime VALUATIONENDDATE { get; set; }
        public decimal OMV { get; set; }
        public decimal FSV { get; set; }
        public string VALUER { get; set; }
        public string COLLATERALDETAILS { get; set; }
        public bool ISINFORMATIONCONFIRMED { get; set; }
        
    }
}
