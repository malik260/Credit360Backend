namespace FintrakBanking.Entities.StagingModels

{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_LOAN_MART
    {
        [Key]
        public int LOANMARTID { get; set; }

        public string BRANCH { get; set; }

        public string SOL_DESC { get; set; }

        public string ACCOUNT { get; set; }

        public string ACCOUNT_NAME { get; set; }

        public string SECTOR { get; set; }

        public string INSIDER_FLAG { get; set; }

        public decimal SANCTIONED_LIMIT { get; set; }

        public string CURRENCY { get; set; }

        public decimal OTHER_INC { get; set; }

        public decimal INT_IN_SUSPENSE { get; set; }

        public string USER_CLASSIFICATION { get; set; }

        public DateTime FAC_GRANT_DATE { get; set; }

        public DateTime EXPIRY_DATE { get; set; }

        public DateTime MATURITY_DATE { get; set; }

        public int INTEREST_RATE { get; set; }

        public decimal FAC_GRANT_AMT { get; set; }

        public string CUST_ID { get; set; }

        public string SUB_SECTOR_CODE { get; set; }

        public string SUB_SECTOR_DESC { get; set; }

        public string SCHEME_CODE { get; set; }

        public decimal INT_RECEIVABLE_AMT { get; set; }

        public decimal INT_DUE { get; set; }

        public decimal TRAN_DATE_BAL { get; set; }

        public decimal OTHER_CHARGES { get; set; }

        public decimal FINAL_BALANCE { get; set; }

        public string SUB_USER_CLASSIFICATION { get; set; }

        public bool SUB_STANDARD { get; set; }

        public bool DOUBTFUL { get; set; }

        public string SCHM_TYPE { get; set; }

        public DateTime CLASSIFICATION_DATE { get; set; }

        public DateTime APPLICATION_DATE { get; set; }

        public decimal LAST_CREDIT_AMT { get; set; }

        public DateTime LAST_CREDIT_DATE { get; set; }

        public DateTime SANCT_LIMIT_DATE { get; set; }

        public DateTime LIM_EXPIRY_DATE { get; set; }

        public decimal DAYS_PAST_DUE { get; set; }

        public decimal SYSTEM_RATE { get; set; }

        public decimal NIFEX_RATE { get; set; }

        public decimal NAFEX_RATE { get; set; }

        public string SCHEME_DESCRIPTION { get; set; }

        public string BU_CODE { get; set; }

        public string BU_DESCRIPTION { get; set; }

        public string GROUP_DESCRIPTION { get; set; }

        public string TEAM_CODE { get; set; }

        public string GROUP_CODE { get; set; }

        public string DESK_CODE { get; set; }

        public string DESK_DESCRIPTION { get; set; }

        public string RM_CODE { get; set; }

        public DateTime ENTRY_DATE { get; set; }

        public string SOURCE { get; set; }

        

        //[Column(Order = 0)]
        ////[StringLength(255)]
        //public string LOANREFERENCENUMBER { get; set; }

        //[Key]
        //[Column(Order = 1, TypeName = "date")]
        //public DateTime REPORTDATE { get; set; }

        ////[StringLength(255)]
        //public string PRODUCTCODE { get; set; }

        ////[StringLength(255)]
        //public string CASAACCOUNT { get; set; }

        ////[StringLength(255)]
        //public string CUSTOMERCODE { get; set; }

        ////[StringLength(255)]
        //public string BRANCHCODE { get; set; }

        //public DateTime? EFFECTIVEDATE { get; set; }

        //public DateTime? MATURITYDATE { get; set; }

        ////[StringLength(255)]
        //public string COMPANYCODE { get; set; }

        //public double? TENOR { get; set; }

        ////[StringLength(255)]
        //public string PRINCIPALFREQUENCYTYPEID { get; set; }

        ////[StringLength(255)]
        //public string INTERESTFREQUENCYTYPEID { get; set; }

        //public double? INTERESTRATE { get; set; }

        ////[Column(TypeName = "money")]
        //public decimal? PRINCIPALAMOUNT { get; set; }

        //public double? PRINCIPALINSTALLMENTLEFT { get; set; }

        //public double? INTERESTINSTALLMENTLEFT { get; set; }

        //public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }

        //public DateTime? FIRSTINTERESTPAYMENTDATE { get; set; }

        ////[Column(TypeName = "money")]
        //public decimal? OUTSTANDINGPRINCIPAL { get; set; }

        ////[Column(TypeName = "money")]
        //public decimal? OUTSTANDINGINTEREST { get; set; }

        ////[StringLength(50)]
        //public string CURRENCY { get; set; }

        //public double? EXCHANGERATE { get; set; }

        ////[StringLength(255)]
        //public string CLASSIFICATION { get; set; }

        ////[StringLength(255)]
        //public string SUBCLASSIFICATION { get; set; }

        ////[Column(TypeName = "money")]
        //public decimal? CASABALANCE { get; set; }

        ////[StringLength(255)]
        //public string STAFFCODE { get; set; }

        ////[StringLength(255)]
        //public string SECTOR { get; set; }

        ////[StringLength(255)]
        //public string SUBSECTOR { get; set; }
    }
}
