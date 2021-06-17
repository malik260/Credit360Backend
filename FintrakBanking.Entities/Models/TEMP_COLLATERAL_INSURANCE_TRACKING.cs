using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TEMP_COLLATERAL_INSURANCE_TRACKING")]
    public class TEMP_COLLATERAL_INSURANCE_TRACKING
    {
        [Key]
        public int COLLATERALINSURANCETRACKINGID { get; set; }
        public int? LOANAPPLICATIONDETAILID { get; set; }
        public int? COLLATERALCUSTOMERID { get; set; }
        public decimal? SUMINSURED { get; set; }
        public decimal? PREMIUMPAID { get; set; }
        public DateTime? INSURANCESTARTDATE { get; set; }
        public DateTime? INSURANCEENDDATE { get; set; }
        public int? INSURANCEPOLICYTYPEID { get; set; }
        public int? INSURANCESTATUSID { get; set; }
        public int? INSURANCECOMPANYID { get; set; }
        public string ISURANCECOMPANYADDRESS { get; set; }
        public string POLICYNUMBER { get; set; }

        public DateTime? VALUATIONSTARTDATE { get; set; }
        public DateTime? VALUATIONENDDATE { get; set; }
        public decimal? OMV { get; set; }
        public decimal? FSV { get; set; }
        public int? VALUERID { get; set; }
        public string COLLATERALDETAILS { get; set; }
        public bool ISINFORMATIONCONFIRMED { get; set; }
        public string OTHERVALUER { get; set; }
        public string OTHERINSURANCECOMPANY { get; set; }
        public string OTHERINSURANCEPOLICYTYPE { get; set; }
        public string GPSCOORDINATES { get; set; }
        public int? COLLATERALTYPE { get; set; }
        public int? COLLATERALSUBTYPE { get; set; }
        public string FIRSTLOSSPAYEE { get; set; }
        public decimal? INSURABLEVALUE { get; set; }
        public string COMMENT { get; set; }
        public bool DELETED { get; set; }
        public int? CREATEDBY { get; set; }
        public int? UPDATEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        public string BATCHCODE { get; set; }
    }
}
