namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_ARCHIVE")]
    public partial class TBL_LOAN_ARCHIVE
    {
        [Key]
        public int LOANARCHIVEID { get; set; }
      
        //[Column(TypeName = "date")]
        public DateTime CHANGEEFFECTIVEDATE { get; set; }

        public bool ISAPPLIED { get; set; }

        public int OPERATIONID { get; set; }

        [Required]
        //[StringLength(500)]
        public string CHANGEREASON { get; set; }

        public double PRODUCTPRICEINDEXRATE { get; set; }

        public int? CUSTOMERRISKRATINGID { get; set; }

        public int LOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public int? CASAACCOUNTID2 { get; set; }

        public short BRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        [Required]
        //[StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        public short SUBSECTORID { get; set; }

        public short? PRINCIPALFREQUENCYTYPEID { get; set; }

        public short? INTERESTFREQUENCYTYPEID { get; set; }

        public int PRINCIPALNUMBEROFINSTALLMENT { get; set; }

        public int INTERESTNUMBEROFINSTALLMENT { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        //[StringLength(50)]
        public string MISCODE { get; set; }

        //[StringLength(50)]
        public string TEAMMISCODE { get; set; }

        public double INTERESTRATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime MATURITYDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime BOOKINGDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? LASTRESTRUCTUREDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal PRINCIPALAMOUNT { get; set; }

        public int PRINCIPALINSTALLMENTLEFT { get; set; }

        public int INTERESTINSTALLMENTLEFT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? APPROVEDBY { get; set; }

        //[StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public short LOANSTATUSID { get; set; }

        public short SCHEDULETYPEID { get; set; }

        public short SCHEDULEDAYCOUNTCONVENTIONID { get; set; }

        public short SCHEDULEDAYINTERESTTYPEID { get; set; }

        public bool ISDISBURSED { get; set; }

        public int? DISBURSEDBY { get; set; }

        //[StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? DISBURSEDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal EQUITYCONTRIBUTION { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? FIRSTINTERESTPAYMENTDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal OUTSTANDINGPRINCIPAL { get; set; }

        //[Column(TypeName = "money")]
        public decimal OUTSTANDINGINTEREST { get; set; }

        //[Column(TypeName = "money")]
        public decimal PASTDUEPRINCIPAL { get; set; }

        //[Column(TypeName = "money")]
        public decimal PASTDUEINTEREST { get; set; }

        //[Column(TypeName = "money")]
        public decimal INTERESTONPASTDUEPRINCIPAL { get; set; }

        //[Column(TypeName = "money")]
        public decimal INTERESTONPASTDUEINTEREST { get; set; }

        //[Column(TypeName = "money")]
        public decimal PENALCHARGEAMOUNT { get; set; }

        public int? PRINCIPALADDITIONCOUNT { get; set; }

        public int? PRINCIPALREDUCTIONCOUNT { get; set; }

        public bool FIXEDPRINCIPAL { get; set; }

        public bool PROFILELOAN { get; set; }

        public bool DISCHARGELETTER { get; set; }

        public bool SUSPENDINTEREST { get; set; }

        public bool? ISSCHEDULEDPREPAYMENT { get; set; }

        public bool ALLOWFORCEDEBITREPAYMENT { get; set; }

        //[Column(TypeName = "money")]
        public decimal? SCHEDULEDPREPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? SCHEDULEDPREPAYMENTDATE { get; set; }

        public short? SCH_PREPAYMENT_FREQUENCY_TYPID { get; set; }

        public int INT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int EXT_PRUDENT_GUIDELINE_STATUSID { get; set; }

        public int USER_PRUDENTIAL_GUIDE_STATUSID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? NPLDATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public string ARCHIVEBATCHCODE { get; set; }
        

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_CASA TBL_CASA { get; set; }

        public virtual TBL_CASA TBL_CASA1 { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_DAY_INTEREST_TYPE TBL_DAY_INTEREST_TYPE { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE1 { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE2 { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_SUB_SECTOR TBL_SUB_SECTOR { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE1 { get; set; }

        public virtual TBL_LOAN_SCHEDULE_TYPE TBL_LOAN_SCHEDULE_TYPE { get; set; }

        public virtual TBL_LOAN_PRUDENTIALGUIDELINE TBL_LOAN_PRUDENTIALGUIDELINE2 { get; set; }

        public virtual TBL_LOAN_SYSTEM_TYPE TBL_LOAN_SYSTEM_TYPE { get; set; }

    }
}
