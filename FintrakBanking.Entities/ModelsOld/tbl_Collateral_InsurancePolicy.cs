namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_InsurancePolicy")]
    public partial class tbl_Collateral_InsurancePolicy
    {
        [Key]
        public int InsurancePolicyId { get; set; }

        public int ColleralCustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string PolicyNo { get; set; }

        [Column(TypeName = "money")]
        public decimal InsuranceAmount { get; set; }

        public DateTime StartDate { get; set; }

        [Column(TypeName = "money")]
        public decimal PremiumAmount { get; set; }

        public DateTime? AssignmentDate { get; set; }

        [StringLength(300)]
        public string InsurerAddress { get; set; }

        [StringLength(500)]
        public string InsurerDetails { get; set; }

        [StringLength(20)]
        public string RenewalFrequency { get; set; }

        public DateTime? NextRenewalDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
    }
}
