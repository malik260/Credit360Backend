namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_RELIEF")]
    public partial class TBL_STAFF_RELIEF
    {
        [Key]
        public int RELIEFID { get; set; }

        public int STAFFID { get; set; }

        public int RELIEFSTAFFID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string RELIEFREASON { get; set; }

        [Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        public bool ISACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }
    }
}
