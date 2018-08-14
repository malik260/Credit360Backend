namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_FEE_SCHEDULE")]
    public partial class TBL_LOAN_FEE_SCHEDULE
    {
        [Key]
        public int PERIODICLOANCHARGEFEEID { get; set; }

        public int LOANCHARGEFEEID { get; set; }

        public int FEENUMBER { get; set; }

        [Column(TypeName = "date")]
        public DateTime FEEDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal FEEAMOUNT { get; set; }

        public virtual TBL_LOAN_FEE TBL_LOAN_FEE { get; set; }
    }
}
