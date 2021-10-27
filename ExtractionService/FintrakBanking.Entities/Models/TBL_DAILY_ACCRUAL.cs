namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DAILY_ACCRUAL")]
    public partial class TBL_DAILY_ACCRUAL
    {
        [Key]
        public int DAILYACCURALID { get; set; }

        [Required]
        //[StringLength(50)]
        public string REFERENCENUMBER { get; set; }

        //[StringLength(50)]
        public string BASEREFERENCENUMBER { get; set; }

        public short CATEGORYID { get; set; }

        public byte TRANSACTIONTYPEID { get; set; }

        public short PRODUCTID { get; set; }

        public int? CHARGEFEEID { get; set; }

        public int COMPANYID { get; set; }

        public short BRANCHID { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal MAINAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        //[Column(TypeName = "date")]

        [Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        public short DAYCOUNTCONVENTIONID { get; set; }

        //[Column(TypeName = "money")]
        public decimal DAILYACCURALAMOUNT { get; set; }

        public decimal DAILYACCURALAMOUNT2 { get; set; }
        public bool REPAYMENTPOSTEDSTATUS { get; set; }
        public DateTime ? DEMANDDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal SYSTEMDATETIME { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_DAILY_ACCRUAL_CATEGORY TBL_DAILY_ACCRUAL_CATEGORY { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_LOAN_TRANSACTION_TYPE TBL_LOAN_TRANSACTION_TYPE { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
