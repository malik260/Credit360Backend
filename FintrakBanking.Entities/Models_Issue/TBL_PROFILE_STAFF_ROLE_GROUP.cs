namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PROFILE_STAFF_ROLE_GROUP")]
    public partial class TBL_PROFILE_STAFF_ROLE_GROUP
    {
        [Key]
        public int STAFFROLEGROUPID { get; set; }

        public int STAFFROLEID { get; set; }

        public short GROUPID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool APPROVALSTATUS { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public virtual TBL_PROFILE_GROUP TBL_PROFILE_GROUP { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}
