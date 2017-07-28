namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_Assessment_Index_Type")]
    public partial class tbl_Risk_Assessment_Index_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Risk_Assessment_Index_Type()
        {
            tbl_Risk_Assessment_Index = new HashSet<tbl_Risk_Assessment_Index>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short IndexTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string IndexTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Risk_Assessment_Index> tbl_Risk_Assessment_Index { get; set; }
    }
}
