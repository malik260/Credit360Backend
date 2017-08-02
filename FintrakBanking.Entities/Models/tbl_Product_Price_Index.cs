namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Product_Price_Index")]
    public partial class tbl_Product_Price_Index
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Product_Price_Index()
        {
            tbl_Product = new HashSet<tbl_Product>();
            tbl_temp_Product = new HashSet<tbl_temp_Product>();
        }

        [Key]
        public short ProductPriceIndexId { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string PriceIndexName { get; set; }

        public double PriceIndexRate { get; set; }

        [Required]
        [StringLength(500)]
        public string PriceIndexDescription { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product> tbl_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_temp_Product> tbl_temp_Product { get; set; }
    }
}
