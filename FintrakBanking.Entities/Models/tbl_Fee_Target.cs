namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Fee_Target")]
    public partial class tbl_Fee_Target
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Fee_Target()
        {
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
            tbl_Fee = new HashSet<tbl_Fee>();
            tbl_Temp_Charge_Fee = new HashSet<tbl_Temp_Charge_Fee>();
            tbl_Temp_Fee = new HashSet<tbl_Temp_Fee>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeTargetId { get; set; }

        [StringLength(50)]
        public string FeeTargetName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Charge_Fee> tbl_Temp_Charge_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Fee> tbl_Temp_Fee { get; set; }
    }
}
