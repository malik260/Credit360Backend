namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_POSTING_TYPE")]
    public partial class TBL_POSTING_TYPE
    {
        public TBL_POSTING_TYPE()
        {
            TBL_CHARGE_FEE_DETAIL = new HashSet<TBL_CHARGE_FEE_DETAIL>();
            TBL_TEMP_CHARGE_FEE_DETAIL = new HashSet<TBL_TEMP_CHARGE_FEE_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short POSTINGTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string POSTINGTYPENAME { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE_DETAIL> TBL_TEMP_CHARGE_FEE_DETAIL { get; set; }
    }
}
