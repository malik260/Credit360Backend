namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Fee_Type")]
    public partial class tbl_Fee_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Fee_Type()
        {
            tbl_Fee = new HashSet<tbl_Fee>();
            tbl_Charge_Fee = new HashSet<tbl_Charge_Fee>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FeeTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FeeTypeName { get; set; }

        public bool ByAmountRequired { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Fee> tbl_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Charge_Fee> tbl_Charge_Fee { get; set; }
    }
}
