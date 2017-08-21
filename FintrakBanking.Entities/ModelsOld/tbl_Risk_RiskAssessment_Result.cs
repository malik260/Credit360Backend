namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_RiskAssessment_Result")]
    public partial class tbl_Risk_RiskAssessment_Result
    {
        [Key]
        public int AssessmentResultId { get; set; }

        public int LoanId { get; set; }

        public decimal? RatingScore { get; set; }

        public short CompanyId { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }
    }
}
