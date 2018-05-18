namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DAILY_ACCRUAL")]
    public partial class TBL_DAILY_ACCRUAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DAILYACCURALID { get; set; }

        [Required]
        [StringLength(50)]
        public string REFERENCENUMBER { get; set; }

        [StringLength(50)]
        public string BASEREFERENCENUMBER { get; set; }

        public int CATEGORYID { get; set; }

        public int TRANSACTIONTYPEID { get; set; }

        public int PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int BRANCHID { get; set; }

        public int CURRENCYID { get; set; }

        public decimal EXCHANGERATE { get; set; }

        public decimal MAINAMOUNT { get; set; }

        public decimal INTERESTRATE { get; set; }

        public int DAYCOUNTCONVENTIONID { get; set; }

        public decimal DAILYACCURALAMOUNT { get; set; }

        public int REPAYMENTPOSTEDSTATUS { get; set; }

        public decimal SYSTEMDATETIME { get; set; }

        public DateTime? DATE_ { get; set; }

        public virtual TBL_BRANCH TBL_BRANCH { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_DAILY_ACCRUAL_CATEGORY TBL_DAILY_ACCRUAL_CATEGORY { get; set; }

        public virtual TBL_DAY_COUNT_CONVENTION TBL_DAY_COUNT_CONVENTION { get; set; }

        public virtual TBL_LOAN_TRANSACTION_TYPE TBL_LOAN_TRANSACTION_TYPE { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
