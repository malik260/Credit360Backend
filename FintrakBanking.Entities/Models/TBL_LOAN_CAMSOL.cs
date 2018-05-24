namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_CAMSOL")]
    public partial class TBL_LOAN_CAMSOL
    {
        [Key]
        public int LOAN_CAMSOLID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public int? LOANID { get; set; }

        [Column(TypeName = "money")]
        public decimal AMOUNTAFFECTED { get; set; }

        public DateTime DATE { get; set; }

        [Required]
        [StringLength(50)]
        public string TYPE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
