namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CALL_MEMO_TYPE")]
    public partial class TBL_CALL_MEMO_TYPE
    {
        public TBL_CALL_MEMO_TYPE()
        {
            TBL_CALL_MEMO = new HashSet<TBL_CALL_MEMO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CALLLIMITTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string NAME { get; set; }

        public virtual ICollection<TBL_CALL_MEMO> TBL_CALL_MEMO { get; set; }
    }
}
