namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_LOAN")]
    public partial class TBL_TEMP_LOAN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TERMLOANID { get; set; }

        public decimal PRODUCTPRICEINDEXRATE { get; set; }

        public int? CUSTOMERRISKRATINGID { get; set; }

        public int LOANSYSTEMTYPEID { get; set; }

        public int CUSTOMERID { get; set; }

        public int PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int? CASAACCOUNTID2 { get; set; }

        public int BRANCHID { get; set; }

        public int SUBSECTORID { get; set; }

        public int CURRENCYID { get; set; }

        public decimal EXCHANGERATE { get; set; }

        [Required]
        [StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string RELATED_LOAN_REFERENCE_NUMBER { get; set; }

        public int? PRINCIPALFREQUENCYTYPEID { get; set; }

        public int? INTERESTFREQUENCYTYPEID { get; set; }

        public int PRINCIPALNUMBEROFINSTALLMENT { get; set; }

        public int INTERESTNUMBEROFINSTALLMENT { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        [StringLength(50)]
        public string MISCODE { get; set; }

        [StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public decimal INTERESTRATE { get; set; }

        public decimal PRINCIPALAMOUNT { get; set; }

        public int PRINCIPALINSTALLMENTLEFT { get; set; }

        public int INTERESTINSTALLMENTLEFT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? APPROVEDBY { get; set; }

        [StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public int LOANSTATUSID { get; set; }

        public int SCHEDULETYPEID { get; set; }

        public int SCHEDULEDAYCOUNTCONVENTIONID { get; set; }

        public int SCHEDULEDAYINTERESTTYPEID { get; set; }

        public int SHOULD_DISBURSE { get; set; }

        public int ISDISBURSED { get; set; }

        public int? DISBURSEDBY { get; set; }

        [StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        public int? OPERATIONID { get; set; }

        public decimal EQUITYCONTRIBUTION { get; set; }

        public decimal OUTSTANDINGPRINCIPAL { get; set; }

        public decimal OUTSTANDINGINTEREST { get; set; }

        public decimal PASTDUEPRINCIPAL { get; set; }

        public decimal PASTDUEINTEREST { get; set; }

        public decimal INTERESTONPASTDUEPRINCIPAL { get; set; }

        public decimal INTERESTONPASTDUEINTEREST { get; set; }

        public decimal PENALCHARGEAMOUNT { get; set; }

        public int? PRINCIPALADDITIONCOUNT { get; set; }

        public int? PRINCIPALREDUCTIONCOUNT { get; set; }

        public int FIXEDPRINCIPAL { get; set; }

        public int PROFILELOAN { get; set; }

        public int DISCHARGELETTER { get; set; }

        public int SUSPENDINTEREST { get; set; }

        public int? ISSCHEDULEDPREPAYMENT { get; set; }

        public int ALLOWFORCEDEBITREPAYMENT { get; set; }

        public decimal? SCHEDULEDPREPAYMENTAMOUNT { get; set; }

        public int? SCH_PREPAYMENT_FREQUENCY_TYPID { get; set; }

        public int INT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int EXT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int USER_PRUDENTIAL_GUIDE_STATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? LASTRESTRUCTUREDATE { get; set; }

        public DateTime? BOOKINGDATE { get; set; }

        public DateTime? MATURITYDATE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public DateTime? NPLDATE { get; set; }

        public DateTime? SCHEDULEDPREPAYMENTDATE { get; set; }

        public DateTime? FIRSTINTERESTPAYMENTDATE { get; set; }

        public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }

        public DateTime? DISBURSEDATE { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_DAY_INTEREST_TYPE TBL_DAY_INTEREST_TYPE { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }

        public virtual TBL_LOAN_SCHEDULE_TYPE TBL_LOAN_SCHEDULE_TYPE { get; set; }

        public virtual TBL_LOAN_STATUS TBL_LOAN_STATUS { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }
    }
}
