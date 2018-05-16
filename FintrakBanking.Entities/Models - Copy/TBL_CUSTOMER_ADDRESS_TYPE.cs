namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_ADDRESS_TYPE")]
    public partial class TBL_CUSTOMER_ADDRESS_TYPE
    {
        public TBL_CUSTOMER_ADDRESS_TYPE()
        {
            TBL_CUSTOMER_ADDRESS = new HashSet<TBL_CUSTOMER_ADDRESS>();
            TBL_TEMP_CUSTOMER_ADDRESS = new HashSet<TBL_TEMP_CUSTOMER_ADDRESS>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ADDRESSTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string ADDRESS_TYPE_NAME { get; set; }

        public virtual ICollection<TBL_CUSTOMER_ADDRESS> TBL_CUSTOMER_ADDRESS { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_ADDRESS> TBL_TEMP_CUSTOMER_ADDRESS { get; set; }
    }
}
