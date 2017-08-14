namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_MIS_Type")]
    public partial class tbl_MIS_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_MIS_Type()
        {
            tbl_MIS_Info = new HashSet<tbl_MIS_Info>();
        }

        [Key]
        public short MISTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string MISType { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_MIS_Info> tbl_MIS_Info { get; set; }
    }
}
