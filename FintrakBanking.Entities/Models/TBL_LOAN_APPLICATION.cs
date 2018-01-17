namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION")]
    public partial class TBL_LOAN_APPLICATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_APPLICATION()
        {
            TBL_CREDIT_APPRAISAL_MEMORANDM = new HashSet<TBL_CREDIT_APPRAISAL_MEMORANDM>();
            TBL_LOAN_CONDITION_PRECEDENT = new HashSet<TBL_LOAN_CONDITION_PRECEDENT>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_APPLICATION_COLLATERL = new HashSet<TBL_LOAN_APPLICATION_COLLATERL>();
            TBL_LOAN_APPLICATION_DETAIL = new HashSet<TBL_LOAN_APPLICATION_DETAIL>();
            TBL_LOAN_APPLICATION_DETL_ARCH = new HashSet<TBL_LOAN_APPLICATION_DETL_ARCH>();
            TBL_LOAN_COLLATERAL_MAPPING = new HashSet<TBL_LOAN_COLLATERAL_MAPPING>();
            TBL_LOAN_GUARANTOR = new HashSet<TBL_LOAN_GUARANTOR>();
            TBL_RISK_ASSESSMENT = new HashSet<TBL_RISK_ASSESSMENT>();
        }

        [Key]
        public int LOANAPPLICATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string APPLICATIONREFERENCENUMBER { get; set; }

        public int? LOANPRELIMINARYEVALUATIONID { get; set; }

        [StringLength(50)]
        public string RELATEDREFERENCENUMBER { get; set; }

        public int COMPANYID { get; set; }

        public int? CUSTOMERID { get; set; }

        public short BRANCHID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        public short LOANTYPEID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        public int CASAACCOUNTID { get; set; }

        [Column(TypeName = "date")]
        public DateTime APPLICATIONDATE { get; set; }

        public double INTERESTRATE { get; set; }

        public int APPLICATIONTENOR { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EFFECTIVEDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EXPIRYDATE { get; set; }

        public int OPERATIONID { get; set; }

        public short? PRODUCTCLASSID { get; set; }

        [Column(TypeName = "money")]
        public decimal APPLICATIONAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal APPROVEDAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal TOTALEXPOSUREAMOUNT { get; set; }

        [Required]
        public string LOANINFORMATION { get; set; }

        [Required]
        [StringLength(50)]
        public string MISCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public bool ISINVESTMENTGRADE { get; set; }

        public bool ISRELATEDPARTY { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

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

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public bool SUBMITTEDFORAPPRAISAL { get; set; }

        public bool CUSTOMERINFOVALIDATED { get; set; }

        [Column(TypeName = "date")]
        public DateTime? APPROVEDDATE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_APPRAISAL_MEMORANDM> TBL_CREDIT_APPRAISAL_MEMORANDM { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONDITION_PRECEDENT> TBL_LOAN_CONDITION_PRECEDENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_COLLATERL> TBL_LOAN_APPLICATION_COLLATERL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETAIL> TBL_LOAN_APPLICATION_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_DETL_ARCH> TBL_LOAN_APPLICATION_DETL_ARCH { get; set; }

        public virtual TBL_LOAN_APPLICATION_STATUS TBL_LOAN_APPLICATION_STATUS { get; set; }

        public virtual TBL_LOAN_PRELIMINARY_EVALUATN TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual TBL_LOAN_TYPE TBL_LOAN_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_GUARANTOR> TBL_LOAN_GUARANTOR { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_RISK_ASSESSMENT> TBL_RISK_ASSESSMENT { get; set; }
    }
}
