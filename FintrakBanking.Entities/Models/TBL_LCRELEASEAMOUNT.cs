namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LCRELEASE_AMOUNT")]
    public partial class TBL_LCRELEASE_AMOUNT
    {
        
        [Key]
        public int LCRELEASEAMOUNTID { get; set; }

        public int LCISSUANCEID { get; set; }

        public decimal? RELEASEAMOUNT { get; set; }

        public int? RELEASEAPPROVALSTATUSID { get; set; }

        public int? RELEASEAPPLICATIONSTATUSID { get; set; }
        public string RELEASEREF { get; set; }

        public DateTime? DATETIMECREATED { get; set; }
        
        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
/*

*/
