namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Fee_Interval")]
    public partial class tbl_Fee_Interval
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Fee_Interval()
        {
            tbl_Fee = new HashSet<tbl_Fee>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeIntervalId { get; set; }

        [StringLength(50)]
        public string FeeIntervalName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }
    }
}
