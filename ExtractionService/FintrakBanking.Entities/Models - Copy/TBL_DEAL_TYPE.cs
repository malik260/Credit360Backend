namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DEAL_TYPE")]
    public partial class TBL_DEAL_TYPE
    {
        public TBL_DEAL_TYPE()
        {
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DEALTYPEID { get; set; }

        [StringLength(50)]
        public string DEALTYPENAME { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
