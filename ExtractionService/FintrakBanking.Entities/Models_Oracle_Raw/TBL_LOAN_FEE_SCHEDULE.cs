namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_FEE_SCHEDULE")]
    public partial class TBL_LOAN_FEE_SCHEDULE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PERIODICLOANCHARGEFEEID { get; set; }

        public int LOANCHARGEFEEID { get; set; }

        public int FEENUMBER { get; set; }

        public decimal FEEAMOUNT { get; set; }

        public DateTime? FEEDATE { get; set; }

        public virtual TBL_LOAN_FEE TBL_LOAN_FEE { get; set; }
    }
}
