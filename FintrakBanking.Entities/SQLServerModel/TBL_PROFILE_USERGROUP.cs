namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_USERGROUP")]
    public partial class TBL_PROFILE_USERGROUP
    {
        [Key]
        public int USERGROUPID { get; set; }

        public int USERID { get; set; }

        public short GROUPID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool APPROVALSTATUS { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public virtual TBL_PROFILE_GROUP TBL_PROFILE_GROUP { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }
    }
}
