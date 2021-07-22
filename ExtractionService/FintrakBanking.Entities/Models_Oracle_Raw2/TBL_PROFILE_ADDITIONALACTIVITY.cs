namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_ADDITIONALACTIVITY")]
    public partial class TBL_PROFILE_ADDITIONALACTIVITY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ADDITIONALACTIVITYID { get; set; }

        public int USERID { get; set; }

        public int ACTIVITYID { get; set; }

        public int CANADD { get; set; }

        public int CANEDIT { get; set; }

        public int CANVIEW { get; set; }

        public int CANDELETE { get; set; }

        public int CANAPPROVE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_PROFILE_ACTIVITY TBL_PROFILE_ACTIVITY { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }
    }
}
