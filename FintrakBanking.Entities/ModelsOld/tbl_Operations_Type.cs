namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Operations_Type")]
    public partial class tbl_Operations_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Operations_Type()
        {
            tbl_Operations = new HashSet<tbl_Operations>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short OperationTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string OperationTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Operations> tbl_Operations { get; set; }
    }
}
