namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Application_Collateral_RefNo")]
    public partial class tbl_Loan_Application_Collateral_RefNo
    {
        [Key]
        public int CollateralRefNoId { get; set; }

        public int CustomerCollateralId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentNumber { get; set; }

        public decimal Worth { get; set; }

        public bool IsBankAccount { get; set; }

        public virtual tbl_Loan_Application_Collateral tbl_Loan_Application_Collateral { get; set; }
    }
}
