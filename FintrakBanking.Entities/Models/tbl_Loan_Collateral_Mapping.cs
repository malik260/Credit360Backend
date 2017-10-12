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
        public int LoanCollateralMappingId { get; set; }

        public int CollateralCustomerId { get; set; }

        public int LoanApplicationId { get; set; }

        public bool IsReleased { get; set; }

        public short? ReleaseApprovalStatusId { get; set; }

        public virtual tbl_Approval_Status tbl_Approval_Status { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
