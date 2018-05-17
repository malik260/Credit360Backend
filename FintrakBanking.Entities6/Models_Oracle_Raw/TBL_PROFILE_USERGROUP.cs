namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PROFILE_USERGROUP")]
    public partial class TBL_PROFILE_USERGROUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int USERGROUPID { get; set; }

        public int USERID { get; set; }

        public int GROUPID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public virtual TBL_PROFILE_GROUP TBL_PROFILE_GROUP { get; set; }

        public virtual TBL_PROFILE_USER TBL_PROFILE_USER { get; set; }
    }
}
