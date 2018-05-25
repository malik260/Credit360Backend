namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_PROFILE_ADTN_ACTIVITY")]
    public partial class TBL_TEMP_PROFILE_ADTN_ACTIVITY
    {
        [Key]
        public int TEMPADDITIONALACTIVITYID { get; set; }

        public int TEMPUSERID { get; set; }

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

        public virtual TBL_TEMP_PROFILE_USER TBL_TEMP_PROFILE_USER { get; set; }
    }
}
