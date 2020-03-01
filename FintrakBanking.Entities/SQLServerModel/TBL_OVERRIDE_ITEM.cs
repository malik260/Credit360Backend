namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_OVERRIDE_ITEM")]
    public partial class TBL_OVERRIDE_ITEM
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_OVERRIDE_ITEM()
        {
            TBL_OVERRIDE_DETAIL = new HashSet<TBL_OVERRIDE_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short OVERRIDE_ITEMID { get; set; }

        [Required]
        //[StringLength(350)]
        public string OVERIDE_ITEMNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_OVERRIDE_DETAIL> TBL_OVERRIDE_DETAIL { get; set; }
    }
}
