namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_ACCOUNT_HISTORY")]
    public partial class TBL_STAFF_ACCOUNT_HISTORY
    {
        [Key]
        public int STAFFREASSIGNMENTID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public int TARGETID { get; set; }

        public int STAFFID { get; set; }

        [Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        [StringLength(2000)]
        public string REASONFORCHANGE { get; set; }

        public int? PREVIOUSSTAFFID { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
