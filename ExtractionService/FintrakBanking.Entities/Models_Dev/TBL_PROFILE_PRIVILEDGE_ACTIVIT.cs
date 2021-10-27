namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_PRIVILEDGE_ACTIVIT")]
    public partial class TBL_PROFILE_PRIVILEDGE_ACTIVIT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int USERACTIVITYPRIVILEDGEID { get; set; }

        public int USERID { get; set; }

        public int ACTIVITYID { get; set; }

        public int PRIVILEDGEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_PROFILE_ACTIVITY TBL_PROFILE_ACTIVITY { get; set; }

        public virtual TBL_PROFILE_PRIVILEDGE TBL_PROFILE_PRIVILEDGE { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }
    }
}
