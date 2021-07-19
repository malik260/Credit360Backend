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
        [Column(Order = 0)]
        [StringLength(255)]
        public string LOANREFERENCENUMBER { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "date")]
        public DateTime REPORTDATE { get; set; }

        [StringLength(255)]
        public string PRODUCTCODE { get; set; }

        [StringLength(255)]
        public string CASAACCOUNT { get; set; }

        [StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        [StringLength(255)]
        public string BRANCHCODE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public DateTime? MATURITYDATE { get; set; }

        [StringLength(255)]
        public string COMPANYCODE { get; set; }

        public double? TENOR { get; set; }

        [StringLength(255)]
        public string PRINCIPALFREQUENCYTYPEID { get; set; }

        [StringLength(255)]
        public string INTERESTFREQUENCYTYPEID { get; set; }

        public double? INTERESTRATE { get; set; }

        [Column(TypeName = "money")]
        public decimal? PRINCIPALAMOUNT { get; set; }

        public double? PRINCIPALINSTALLMENTLEFT { get; set; }

        public double? INTERESTINSTALLMENTLEFT { get; set; }

        public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }

        public DateTime? FIRSTINTERESTPAYMENTDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal? OUTSTANDINGPRINCIPAL { get; set; }

        [Column(TypeName = "money")]
        public decimal? OUTSTANDINGINTEREST { get; set; }

        [StringLength(50)]
        public string CURRENCY { get; set; }

        public double? EXCHANGERATE { get; set; }

        [StringLength(255)]
        public string CLASSIFICATION { get; set; }

        [StringLength(255)]
        public string SUBCLASSIFICATION { get; set; }

        [Column(TypeName = "money")]
        public decimal? CASABALANCE { get; set; }

        [StringLength(255)]
        public string STAFFCODE { get; set; }

        [StringLength(255)]
        public string SECTOR { get; set; }

        [StringLength(255)]
        public string SUBSECTOR { get; set; }
    }
}
