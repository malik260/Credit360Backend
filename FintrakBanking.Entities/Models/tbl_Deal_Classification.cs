namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treasury.tbl_Deal_Classification")]
    public partial class tbl_Deal_Classification
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Deal_Classification()
        {
            tbl_Product = new HashSet<tbl_Product>();
            tbl_Product_Type = new HashSet<tbl_Product_Type>();
        }

        [Key]
        public short DealClassificationID { get; set; }

        [Required]
        [StringLength(50)]
        public string Classification { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product> tbl_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Type> tbl_Product_Type { get; set; }
    }
}
