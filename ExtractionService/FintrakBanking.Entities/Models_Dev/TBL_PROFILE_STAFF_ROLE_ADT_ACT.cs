namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_STAFF_ROLE_ADT_ACT")]
    public partial class TBL_PROFILE_STAFF_ROLE_ADT_ACT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STAFFROLEADDITIONALACTIVITYID { get; set; }

        public int STAFFROLEID { get; set; }

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

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}
