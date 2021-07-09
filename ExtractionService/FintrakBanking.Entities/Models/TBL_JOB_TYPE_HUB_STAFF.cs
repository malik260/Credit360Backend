namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_HUB_STAFF")]
    public partial class TBL_JOB_TYPE_HUB_STAFF
    {
       

        [Key]
        public short HUBSTAFFID { get; set; }

        public short JOBTYPEHUBID { get; set; }

        public int STAFFID { get; set; }

        public short JOBTYPEUNITID { get; set; }

        public bool ISTEAMLEAD { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

    }
}





