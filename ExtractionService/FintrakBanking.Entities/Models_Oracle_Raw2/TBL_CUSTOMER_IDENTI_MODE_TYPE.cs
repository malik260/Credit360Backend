namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_IDENTI_MODE_TYPE")]
    public partial class TBL_CUSTOMER_IDENTI_MODE_TYPE
    {
        public TBL_CUSTOMER_IDENTI_MODE_TYPE()
        {
            TBL_CUSTOMER_IDENTIFICATION = new HashSet<TBL_CUSTOMER_IDENTIFICATION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IDENTIFICATIONMODEID { get; set; }

        [Required]
        [StringLength(50)]
        public string IDENTIFICATIONMODE { get; set; }

        public virtual ICollection<TBL_CUSTOMER_IDENTIFICATION> TBL_CUSTOMER_IDENTIFICATION { get; set; }
    }
}
