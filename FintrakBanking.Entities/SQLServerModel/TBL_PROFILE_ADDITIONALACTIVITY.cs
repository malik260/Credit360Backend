namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_ADDITIONALACTIVITY")]
    public partial class TBL_PROFILE_ADDITIONALACTIVITY
    {
        [Key]
        public int ADDITIONALACTIVITYID { get; set; }

        public int USERID { get; set; }

        public int ACTIVITYID { get; set; }

        public bool CANADD { get; set; }

        public bool CANEDIT { get; set; }

        public bool CANVIEW { get; set; }

        public bool CANDELETE { get; set; }

        public bool CANAPPROVE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_PROFILE_ACTIVITY TBL_PROFILE_ACTIVITY { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }

        public DateTime? EXPIREON { get; set; }
    }
}
