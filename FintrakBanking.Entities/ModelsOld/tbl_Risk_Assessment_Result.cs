namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_Assessment_Result")]
    public partial class tbl_Risk_Assessment_Result
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RiskAssessmentId { get; set; }

        public int RatingIndexId { get; set; }

        public int LoanId { get; set; }

        public decimal IndexScore { get; set; }

        public int CompanyId { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }
    }
}
