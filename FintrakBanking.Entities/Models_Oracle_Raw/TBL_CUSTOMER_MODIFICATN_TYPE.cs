namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_MODIFICATN_TYPE")]
    public partial class TBL_CUSTOMER_MODIFICATN_TYPE
    {
        public TBL_CUSTOMER_MODIFICATN_TYPE()
        {
            TBL_CUSTOMER_MODIFICATION = new HashSet<TBL_CUSTOMER_MODIFICATION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MODIFICATIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string MODIFICATIONTYPENAME { get; set; }

        public virtual ICollection<TBL_CUSTOMER_MODIFICATION> TBL_CUSTOMER_MODIFICATION { get; set; }
    }
}
