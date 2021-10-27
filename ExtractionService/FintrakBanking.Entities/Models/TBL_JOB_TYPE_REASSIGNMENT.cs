namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_REASSIGNMENT")]
    public partial class TBL_JOB_TYPE_REASSIGNMENT
    {
        [Key]
        public short REASSIGNMENTID { get; set; }
        public short JOBTYPEID { get; set; }
        public int STAFFID { get; set; }
        public int COMPANYID { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

    }
}
