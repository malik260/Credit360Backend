namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Relationship_Officer_History")]
    public partial class tbl_Loan_Relationship_Officer_History
    {
        [Key]
        public short LoanRelationshipOfficerId { get; set; }

        public int LoanId { get; set; }

        public int StaffId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool? IsCurrent { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }
    }
}
