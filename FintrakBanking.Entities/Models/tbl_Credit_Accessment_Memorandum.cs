namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Accessment_Memorandum")]
    public partial class tbl_Credit_Accessment_Memorandum
    {
        [Key]
        public int AccessmentMemorandumId { get; set; }

        public int LoanApplicationId { get; set; }

        [Required]
        [StringLength(50)]
        public string CAMRef { get; set; }

        public bool IsSubmitted { get; set; }

        public bool IsProccessed { get; set; }

        public bool HasBeenRiskRated { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [Column(TypeName = "ntext")]
        [Required]
        public string CAMDocumentation { get; set; }
    }
}
