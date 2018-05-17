namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_ACTIVITY_PARENT")]
    public partial class TBL_PROFILE_ACTIVITY_PARENT
    {
        public TBL_PROFILE_ACTIVITY_PARENT()
        {
            TBL_PROFILE_ACTIVITY = new HashSet<TBL_PROFILE_ACTIVITY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACTIVITYPARENTID { get; set; }

        [Required]
        [StringLength(200)]
        public string ACTIVITYPARENTNAME { get; set; }

        public virtual ICollection<TBL_PROFILE_ACTIVITY> TBL_PROFILE_ACTIVITY { get; set; }
    }
}
