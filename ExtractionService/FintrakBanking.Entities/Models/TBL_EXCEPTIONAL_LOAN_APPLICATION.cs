using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_EXCEPTIONAL_LOAN_APPLICATION")]
    public class TBL_EXCEPTIONAL_LOAN_APPLICATION
    {
        public TBL_EXCEPTIONAL_LOAN_APPLICATION()
        {
            TBL_EXCEPTIONAL_LOAN_APPL_DETAIL = new HashSet<TBL_EXCEPTIONAL_LOAN_APPL_DETAIL>();
        }

        [Key]
        public int EXCEPTIONALLOANAPPLICATIONID { get; set; }

        [Required]
        public string APPLICATIONREFERENCENUMBER { get; set; }

        public int? LOANPRELIMINARYEVALUATIONID { get; set; }

        public int? LOANTERMSHEETID { get; set; }

        public string RELATEDREFERENCENUMBER { get; set; }

        public int COMPANYID { get; set; }

        public int? CUSTOMERID { get; set; }

        public short BRANCHID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        public short LOANAPPLICATIONTYPEID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public DateTime APPLICATIONDATE { get; set; }

        public double INTERESTRATE { get; set; }

        public int APPLICATIONTENOR { get; set; }

        public int OPERATIONID { get; set; }

        public short? PRODUCTCLASSID { get; set; }
        public int? PRODUCTID { get; set; }

        public short PRODUCT_CLASS_PROCESSID { get; set; }

        public decimal APPLICATIONAMOUNT { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        public decimal TOTALEXPOSUREAMOUNT { get; set; }

        public string APIREQUESTID { get; set; }

        [Required]
        public string LOANINFORMATION { get; set; }

        [Required]
        public string MISCODE { get; set; }

        [Required]
        public string TEAMMISCODE { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public bool ISRELATEDPARTY { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public bool? ISLINEFACILITY { get; set; }
        public bool ISPROJECTRELATED { get; set; }
        public bool ISONLENDING { get; set; }
        public bool ISINTERVENTIONFUNDS { get; set; }
        public bool ISORRBASEDAPPROVAL { get; set; }
        public bool WITHINSTRUCTION { get; set; }
        public bool DOMICILIATIONNOTINPLACE { get; set; }

        public int OWNEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public short APPLICATIONSTATUSID { get; set; }

        public int? FINALAPPROVAL_LEVELID { get; set; }

        public short? TRANCHEAPPROVAL_LEVELID { get; set; }

        public short? NEXTAPPLICATIONSTATUSID { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public int? RISKRATINGID { get; set; }

        public bool SUBMITTEDFORAPPRAISAL { get; set; }

        public bool CUSTOMERINFOVALIDATED { get; set; }

        public DateTime? APPROVEDDATE { get; set; }

        public DateTime? AVAILMENTDATE { get; set; }

        public bool DISPUTED { get; set; }

        public bool REQUIRECOLLATERAL { get; set; }

        public string COLLATERALDETAIL { get; set; }

        public int? CAPREGIONID { get; set; }

        public short? REQUIRECOLLATERALTYPEID { get; set; }

        public bool? ISCHECKLISTLOADED { get; set; }

        public bool ISADHOCAPPLICATION { get; set; }

        public decimal? LOANSWITHOTHERS { get; set; }

        public string OWNERSHIPSTRUCTURE { get; set; }

        public int? LOANAPPROVEDLIMITID { get; set; }

        public int? FLOWCHANGEID { get; set; }

        public bool? ISMULTIPLEPRODUCTDRAWDOWN { get; set; }
        public short? APPROVEDLINESTATUSID { get; set; }
        public bool ISEMPLOYERRELATED { get; set; }
        public int? RELATEDEMPLOYERID { get; set; }
        public int CREATEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_EXCEPTIONAL_LOAN_APPL_DETAIL> TBL_EXCEPTIONAL_LOAN_APPL_DETAIL { get; set; }
    }
}
