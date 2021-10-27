namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_PRIVILEDGE")]
    public partial class TBL_PROFILE_PRIVILEDGE
    {
        public TBL_PROFILE_PRIVILEDGE()
        {
            TBL_PROFILE_PRIVILEDGE_ACTIVIT = new HashSet<TBL_PROFILE_PRIVILEDGE_ACTIVIT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRIVILEDGEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRIVILEDGENAME { get; set; }

        public virtual ICollection<TBL_PROFILE_PRIVILEDGE_ACTIVIT> TBL_PROFILE_PRIVILEDGE_ACTIVIT { get; set; }
    }
}
