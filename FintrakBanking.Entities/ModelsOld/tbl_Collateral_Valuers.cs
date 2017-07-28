namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Valuers")]
    public partial class tbl_Collateral_Valuers
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Valuers()
        {
            tbl_Collateral_Customer = new HashSet<tbl_Collateral_Customer>();
        }

        [Key]
        public short CollateralValuerId { get; set; }

        [Required]
        [StringLength(50)]
        public string ValuerLicenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public short? ValuerTypeId { get; set; }

        public short? CityId { get; set; }

        public short? CountryId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Customer> tbl_Collateral_Customer { get; set; }

        public virtual tbl_Collateral_ValuerType tbl_Collateral_ValuerType { get; set; }
    }
}
