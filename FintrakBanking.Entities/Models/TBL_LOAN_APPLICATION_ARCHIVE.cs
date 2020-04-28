namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_ARCHIVE")]
    public partial class TBL_LOAN_APPLICATION_ARCHIVE
    {
        [Key]
        public int APPLICATION_ARCHIVEID { get; set; }
        //[Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        [Required]
        //[StringLength(50)]
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public int? LOANPRELIMINARYEVALUATIONID { get; set; }
        public int? LOANTERMSHEETID { get; set; }
        public int COMPANYID { get; set; }
        public int? CUSTOMERID { get; set; }
        public short BRANCHID { get; set; }
        public int? CUSTOMERGROUPID { get; set; }
        public short LOANAPPLICATIONTYPEID { get; set; }
        public int RELATIONSHIPOFFICERID { get; set; }
        public int RELATIONSHIPMANAGERID { get; set; }
        public int? CASAACCOUNTID { get; set; }
        //[Column(TypeName = "date")]
        public DateTime? APPLICATIONDATE { get; set; }
        public double INTERESTRATE { get; set; }
        public int APPLICATIONTENOR { get; set; }
        //[Column(TypeName = "date")]
        public DateTime? EFFECTIVEDATE { get; set; }
        //[Column(TypeName = "date")]
        public DateTime? EXPIRYDATE { get; set; }
        public int OPERATIONID { get; set; }
        public short? PRODUCTCLASSID { get; set; }
        public int? PRODUCTID { get; set; }
        public short PRODUCT_CLASS_PROCESSID { get; set; }
        //[Column(TypeName = "money")]
        public decimal APPLICATIONAMOUNT { get; set; }
        //[Column(TypeName = "money")]
        public decimal APPROVEDAMOUNT { get; set; }
        public decimal TOTALEXPOSUREAMOUNT { get; set; }
        public string APIREQUESTID { get; set; }
        [Required]
        public string LOANINFORMATION { get; set; }
        [Required]
        //[StringLength(50)]
        public string MISCODE { get; set; }
        [Required]
        //[StringLength(50)]
        public string TEAMMISCODE { get; set; }
        public bool ISINVESTMENTGRADE { get; set; }
        public bool ISRELATEDPARTY { get; set; }
        public bool ISPOLITICALLYEXPOSED { get; set; }
        public bool ISPROJECTRELATED { get; set; }
        public bool ISONLENDING { get; set; }
        public bool ISINTERVENTIONFUNDS { get; set; }
        public bool ISORRBASEDAPPROVAL { get; set; }
        public bool WITHINSTRUCTION { get; set; }
        public bool DOMICILIATIONNOTINPLACE { get; set; }
        public int CREATEDBY { get; set; }
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
        //[Column(TypeName = "date")]
        public DateTime? AVAILMENTDATE { get; set; }
        public bool DISPUTED { get; set; }
        public bool REQUIRECOLLATERAL { get; set; }
        //[StringLength(2000)]
        public string COLLATERALDETAIL { get; set; }
        public int? CAPREGIONID { get; set; }
        public short? REQUIRECOLLATERALTYPEID { get; set; }
        public string RELATEDREFERENCENUMBER { get; set; }
        public bool? ISCHECKLISTLOADED { get; set; }
        public bool ISADHOCAPPLICATION { get; set; }
        public decimal? LOANSWITHOTHERS { get; set; }
        public string OWNERSHIPSTRUCTURE { get; set; }
        public int? LOANAPPROVEDLIMITID { get; set; }
        public int? FLOWCHANGEID { get; set; }
        public bool? ISMULTIPLEPRODUCTDRAWDOWN { get; set; }
        public int? ARCHIVINGOPERATIONID { get; set; }
        public bool NOTINNEGATIVECRMS { get; set; }
        public bool NOTINBLACKBOOK { get; set; }
        public bool NOTINCAMSOL { get; set; }
        public bool NOTINXDS { get; set; }
        public bool NOTINCRC { get; set; }
        public int? ARCHIVEDBY { get; set; }
        public short? APPROVEDLINESTATUSID { get; set; }
        public int OWNEDBY { get; set; }
        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION_STATUS TBL_LOAN_APPLICATION_STATUS { get; set; }

        public virtual TBL_LOAN_PRELIMINARY_EVALUATN TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual TBL_LOAN_APPLICATION_TYPE TBL_LOAN_APPLICATION_TYPE { get; set; }
    }
}
