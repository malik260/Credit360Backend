namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_GROUP_RELATN_TYPE")]
    public partial class TBL_CUSTOMER_GROUP_RELATN_TYPE
    {
        public TBL_CUSTOMER_GROUP_RELATN_TYPE()
        {
            TBL_CUSTOMER_GROUP_MAPPING = new HashSet<TBL_CUSTOMER_GROUP_MAPPING>();
            TBL_TEMP_CUSTOMER_GROUP_MAPPNG = new HashSet<TBL_TEMP_CUSTOMER_GROUP_MAPPNG>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RELATIONSHIPTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string RELATIONSHIPTYPENAME { get; set; }

        public virtual ICollection<TBL_CUSTOMER_GROUP_MAPPING> TBL_CUSTOMER_GROUP_MAPPING { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> TBL_TEMP_CUSTOMER_GROUP_MAPPNG { get; set; }
    }
}
