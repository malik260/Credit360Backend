namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Condition_Precedent")]
    public partial class tbl_Loan_Condition_Precedent
    {
        [Key]
        public int ConditionId { get; set; }

        [Required]
        [StringLength(500)]
        public string Condition { get; set; }

        public bool? IsExternal { get; set; }

        public int CreatedBy { get; set; }

        public int LoanApplicationId { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
