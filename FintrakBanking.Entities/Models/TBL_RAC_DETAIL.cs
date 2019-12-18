namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_DETAIL")]
    public partial class TBL_RAC_DETAIL
    {
        [Key]
        public int RACDETAILID { get; set; }

        public int RACDEFINITIONID { get; set; }

        public int OPERATIONID { get; set; }

        public int TARGETID { get; set; }

        [Required]
        //[StringLength(100)]
        public string ACTUALVALUE { get; set; }

        public int CHECKLISTSTATUS { get; set; }

        public int CHECKLISTSTATUS2 { get; set; }

        public int CHECKLISTSTATUS3 { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_RAC_DEFINITION TBL_RAC_DEFINITION { get; set; }

    }
}