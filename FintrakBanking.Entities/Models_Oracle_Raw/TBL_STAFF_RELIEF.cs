namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STAFF_RELIEF")]
    public partial class TBL_STAFF_RELIEF
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RELIEFID { get; set; }

        public int STAFFID { get; set; }

        public int RELIEFSTAFFID { get; set; }

        [Required]
        [StringLength(2000)]
        public string RELIEFREASON { get; set; }

        public int ISACTIVE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? ENDDATE { get; set; }

        public DateTime? STARTDATE { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
