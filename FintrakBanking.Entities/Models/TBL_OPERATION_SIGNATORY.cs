namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_OPERATION_SIGNATORY")]
    public partial class TBL_OPERATION_SIGNATORY
    {
        [Key]
        public int OPERATIONSIGNATORYID { get; set; }
        public int OPERATIONID { get; set; }
        public int? TARGETID { get; set; }
        [ForeignKey("TBL_AUTHORISED_SIGNATORY")]
        public int SIGNATORYID { get; set; }
        public int POSITION { get; set; }


        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public virtual TBL_AUTHORISED_SIGNATORY TBL_AUTHORISED_SIGNATORY { get; set; }
    }
}

