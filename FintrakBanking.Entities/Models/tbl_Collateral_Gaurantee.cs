namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Gaurantee")]
    public partial class tbl_Collateral_Gaurantee
    {
        [Key]
        public int GauranteeId { get; set; }

        public int ColleralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string GuaranteeType { get; set; }

        [Column(TypeName = "money")]
        public decimal GuaranteeAmount { get; set; }

        [StringLength(20)]
        public string GuarantorCIFNo { get; set; }

        [Required]
        [StringLength(50)]
        public string GuarantorName { get; set; }

        [Required]
        [StringLength(250)]
        public string GuarantorAddress { get; set; }

        public DateTime AgreementDate { get; set; }

        [StringLength(100)]
        public string ContinuingGuarantee { get; set; }

        [StringLength(200)]
        public string GuarantorOwnExposure { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalGuaranteeAmount { get; set; }

        public bool Revokeable { get; set; }

        public DateTime? RevokeDate { get; set; }

        public byte? Rating { get; set; }

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
