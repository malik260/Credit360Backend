namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_ARCHIVE")]
    public partial class TBL_LOAN_APPLICATION_ARCHIVE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPLICATION_ARCHIVEID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string APPLICATIONREFERENCENUMBER { get; set; }

        public int? LOANPRELIMINARYEVALUATIONID { get; set; }

        public int COMPANYID { get; set; }

        public int? CUSTOMERID { get; set; }

        public int BRANCHID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        public int LOANAPPLICATIONTYPEID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public decimal INTERESTRATE { get; set; }

        public int APPLICATIONTENOR { get; set; }

        public int OPERATIONID { get; set; }

        public int? PRODUCTCLASSID { get; set; }

        public decimal APPLICATIONAMOUNT { get; set; }

        public decimal APPROVEDAMOUNT { get; set; }

        [Required]
        public string LOANINFORMATION { get; set; }

        [Required]
        [StringLength(50)]
        public string MISCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public int ISINVESTMENTGRADE { get; set; }

        public int ISRELATEDPARTY { get; set; }

        public int ISPOLITICALLYEXPOSED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int APPLICATIONSTATUSID { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public int SUBMITTEDFORAPPRAISAL { get; set; }

        public int CUSTOMERINFOVALIDATED { get; set; }

        public int NOTINNEGATIVECRMS { get; set; }

        public int NOTINBLACKBOOK { get; set; }

        public int NOTINCAMSOL { get; set; }

        public int NOTINXDS { get; set; }

        public int NOTINCRC { get; set; }

        public DateTime? EXPIRYDATE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public DateTime APPLICATIONDATE { get; set; }

        public DateTime ARCHIVEDATE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_LOAN_APPLICATION_TYPE TBL_LOAN_APPLICATION_TYPE { get; set; }

        public virtual TBL_LOAN_APPLICATION_STATUS TBL_LOAN_APPLICATION_STATUS { get; set; }

        public virtual TBL_LOAN_PRELIMINARY_EVALUATN TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual TBL_PRODUCT_CLASS TBL_PRODUCT_CLASS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }
    }
}
