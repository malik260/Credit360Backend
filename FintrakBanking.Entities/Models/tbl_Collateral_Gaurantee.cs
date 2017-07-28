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

        public short CollateralSubTypeId { get; set; }

<<<<<<< HEAD
        [StringLength(20)]
        public string GuarantorCIFNo { get; set; }
=======
        public bool? IsOwnedByCustomer { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

        [Required]
        [StringLength(50)]
        public string InstitutionName { get; set; }

        [Required]
        [StringLength(250)]
        public string GuarantorAddress { get; set; }

        [StringLength(50)]
        public string GuarantorReferenceNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string GuaranteeType { get; set; }

        [Column(TypeName = "money")]
        public decimal GuaranteeValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

<<<<<<< HEAD
        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
=======
        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba
    }
}
