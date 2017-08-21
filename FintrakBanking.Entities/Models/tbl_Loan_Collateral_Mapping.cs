namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Collateral_Mapping")]
    public partial class tbl_Loan_Collateral_Mapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LoanCollateralMappingId { get; set; }

        public int LoanId { get; set; }

        public int CollateralCustomerId { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
