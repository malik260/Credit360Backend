namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_MODIFICATN_TYPE")]
    public partial class TBL_CUSTOMER_MODIFICATN_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_MODIFICATN_TYPE()
        {
            TBL_CUSTOMER_MODIFICATION = new HashSet<TBL_CUSTOMER_MODIFICATION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MODIFICATIONTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string MODIFICATIONTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_MODIFICATION> TBL_CUSTOMER_MODIFICATION { get; set; }
    }
}
