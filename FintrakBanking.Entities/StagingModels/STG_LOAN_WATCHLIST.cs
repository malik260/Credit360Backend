namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_LOAN_WATCHLIST")]
    public partial class STG_LOAN_WATCHLIST
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        //[StringLength(255)]
        public string PRODUCTCODE { get; set; }

        //[StringLength(255)]
        public string CASAACCOUNT { get; set; }

        //[StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        //[StringLength(255)]
        public string BRANCHCODE { get; set; }

        //[StringLength(255)]
        public string LOANREFERENCENUMBER { get; set; }

        public int? TENOR { get; set; }

        public int? PRINCIPALFREQUENCYTYPEID { get; set; }

        public int? INTERESTFREQUENCYTYPEID { get; set; }

        public int? FEEFREQUENCYTYPEID { get; set; }

        //[Column(TypeName = "float")]
        public float INTERESTRATE { get; set; }
        //public decimal? INTERESTRATE { get; set; }

        public DateTime? EFFECTIVEDATE { get; set; }

        public decimal? PRINCIPALAMOUNT { get; set; }

        public int? PRINCIPALINSTALLMENTLEFT { get; set; }

        public int? INTERESTINSTALLMENTLEFT { get; set; }

        public DateTime? FIRSTPRINCIPALPAYMENTDATE { get; set; }

        public DateTime? FIRSTINTERESTPAYMENTDATE { get; set; }

        public decimal? OUTSTANDINGPRINCIPAL { get; set; }

        //[StringLength(255)]
        public string CLASSIFICATION { get; set; }

        //[StringLength(255)]
        public string SUBCLASSIFICATION { get; set; }
    }
}
