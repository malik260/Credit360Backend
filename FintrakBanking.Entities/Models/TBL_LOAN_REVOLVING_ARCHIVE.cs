namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVOLVING_ARCHIVE")]
    public partial class TBL_LOAN_REVOLVING_ARCHIVE
    {
        [Key]
        public int REVOLVINGLOAN_ARCHIVE_ID { get; set; }

        [Column(TypeName = "date")]
        public DateTime ARCHIVEDATE { get; set; }

        public int REVOLVINGLOANID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int? CASAACCOUNTID2 { get; set; }

        public short BRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public double EXCHANGERATE { get; set; }

        [Required]
        [StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string RELATED_LOAN_REFERENCE_NUMBER { get; set; }

        public short SUBSECTORID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        [StringLength(50)]
        public string MISCODE { get; set; }

        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public double INTERESTRATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime MATURITYDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime BOOKINGDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal OVERDRAFTLIMIT { get; set; }

        [Column(TypeName = "money")]
        public decimal? DISBURSED_AMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal? INTEREST_AMOUNT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        [StringLength(50)]
        public string APPROVEDBY { get; set; }

        [StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public short LOANSTATUSID { get; set; }

        public bool ISDISBURSED { get; set; }

        [StringLength(50)]
        public string DISBURSEDBY { get; set; }

        [StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DISBURSEDATE { get; set; }

        public int? OPERATIONID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        public short LOANTYPEID { get; set; }

        [StringLength(50)]
        public string TRANCHEBATCHCODE { get; set; }

        public bool DISCHARGELETTER { get; set; }

        public bool SUSPENDINTEREST { get; set; }

        public short DAYCOUNTCONVENTIONID { get; set; }

        public short CUSTOMERSENSITIVITYLEVELID { get; set; }

        public int? INT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int? EXT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? NPLDATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CASA TBL_CASA1 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }

        public virtual TBL_CUSTOMER_SENSITIVITY_LEVEL TBL_CUSTOMER_SENSITIVITY_LEVEL { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE1 { get; set; }

        public virtual TBL_LOAN_REVOLVING TBL_LOAN_REVOLVING { get; set; }

        public virtual TBL_LOAN_STATUS TBL_LOAN_STATUS { get; set; }

        public virtual TBL_LOAN_TYPE TBL_LOAN_TYPE { get; set; }
    }
}
