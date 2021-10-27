namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_BRANCH_REGION_STAFF")]
    public partial class TBL_BRANCH_REGION_STAFF
    {
        [Key]
        public int STAFFREGIONID { get; set; }

        public int REGIONID { get; set; }

        public int STAFFID { get; set; }

        public int REGIONSTAFFTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_BRANCH_REGION TBL_BRANCH_REGION { get; set; }

    }
}
