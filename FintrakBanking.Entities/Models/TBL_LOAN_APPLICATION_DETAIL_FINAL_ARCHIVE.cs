using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE")]
    public partial class TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE
    {
        [Key]
        public int LOANAPPLICATIONDETAILFINALID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public DateTime? ARCHIVEDATE { get; set; }
        public int LOANAPPLICATIONID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PROPOSEDPRODUCTID { get; set; }

        public int PROPOSEDTENOR { get; set; }

        public double PROPOSEDINTERESTRATE { get; set; }

        public decimal PROPOSEDAMOUNT { get; set; }

        public short APPROVEDPRODUCTID { get; set; }

        public int APPROVEDTENOR { get; set; }

        public double APPROVEDINTERESTRATE { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        public short SUBSECTORID { get; set; }

        public short STATUSID { get; set; }

        public string LOANPURPOSE { get; set; }

        public decimal? EQUITYAMOUNT { get; set; }

        public int? EQUITYCASAACCOUNTID { get; set; }

        public short CONSESSIONAPPROVALSTATUSID { get; set; }

        public string CONSESSIONREASON { get; set; }
        public bool? ISLINEFACILITY { get; set; }
        public bool HASDONECHECKLIST { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public string REPAYMENTTERMS { get; set; }

        public int? REPAYMENTSCHEDULEID { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public bool? ISTAKEOVERAPPLICATION { get; set; }
        public bool? CURRENTSTATUS { get; set; }

        public DateTime? EXPIRYDATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int? OPERATINGCASAACCOUNTID { get; set; }

        public bool SECUREDBYCOLLATERAL { get; set; }
        public int? CRMSCOLLATERALTYPEID { get; set; }
        public int? MORATORIUMDURATION { get; set; }

        public int? CRMSFUNDINGSOURCEID { get; set; }

        public int? CRMSREPAYMENTSOURCEID { get; set; }
        public string CRMSFUNDINGSOURCECATEGORY { get; set; }
        public string CRMS_ECCI_NUMBER { get; set; }
        public string CRMSCODE { get; set; }
        public short? CRMSREPAYMENTAGREEMENTID { get; set; }
        public bool? CRMSVALIDATED { get; set; }

        public DateTime? CRMSDATE { get; set; }

        public string TRANSACTIONDYNAMICS { get; set; }

        public string CONDITIONPRECIDENT { get; set; }

        public string CONDITIONSUBSEQUENT { get; set; }
        public string FIELD1 { get; set; }

        public double? PRODUCTPRICEINDEXRATE { get; set; }

        public short? PRODUCTPRICEINDEXID { get; set; }
        public string FIELD2 { get; set; }
        public decimal? FIELD3 { get; set; }

        public bool ISSPECIALISED { get; set; }

        public int? TENORFREQUENCYTYPEID { get; set; }
        public bool? ISFACILITYCREATED { get; set; }
        public int LOANDETAILREVIEWTYPEID { get; set; }
        public bool? ISFEETAKEN { get; set; }
        public short? TAKEFEETYPEID { get; set; }

        public short? APPROVEDLINESTATUSID { get; set; }

        
    }
}
