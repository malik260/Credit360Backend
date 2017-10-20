namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Product_Group")]
    public partial class tbl_Product_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Product_Group()
        {
            tbl_Product_Type = new HashSet<tbl_Product_Type>();
        }

        [Key]
        public short ProductGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductGroupCode { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductGroupName { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public bool Deleted { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Product_Type> tbl_Product_Type { get; set; }
    }
}
