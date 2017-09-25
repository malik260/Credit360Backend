namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Valuer")]
    public partial class tbl_Collateral_Valuer
    {
        [Key]
        public short CollateralValuerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ValuerLicenceNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public short? ValuerTypeId { get; set; }

        public int? CompanyId { get; set; }

        public short? CityId { get; set; }

        public short? CountryId { get; set; }

        [StringLength(50)]
        public string EmailAddress { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Collateral_Valuer_Type tbl_Collateral_Valuer_Type { get; set; }
    }
}
