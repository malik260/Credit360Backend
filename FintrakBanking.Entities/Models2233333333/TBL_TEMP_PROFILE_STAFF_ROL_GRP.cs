namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_PROFILE_STAFF_ROL_GRP")]
    public partial class TBL_TEMP_PROFILE_STAFF_ROL_GRP
    {
        [Key]
        public int TEMPSTAFFROLEGROUPID { get; set; }

        public int STAFFROLEID { get; set; }

        public short GROUPID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual TBL_PROFILE_GROUP TBL_PROFILE_GROUP { get; set; }

        public virtual TBL_STAFF_ROLE TBL_STAFF_ROLE { get; set; }
    }
}
