namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_LoanApplication_Collateral_Mapping")]
    public partial class tbl_LoanApplication_Collateral_Mapping
    {
        [Key]
        public int LoanApplicationCollateralMappingId { get; set; }

        public int? LoanApplicationId { get; set; }

        public int? CollateralCustomerId { get; set; }

        [StringLength(50)]
        public string CamReferenceNumber { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_LoanApplication_Collateral_Mapping tbl_LoanApplication_Collateral_Mapping1 { get; set; }

        public virtual tbl_LoanApplication_Collateral_Mapping tbl_LoanApplication_Collateral_Mapping2 { get; set; }
    }
}
