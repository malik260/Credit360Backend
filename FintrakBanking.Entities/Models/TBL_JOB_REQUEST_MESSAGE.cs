namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_REQUEST_MESSAGE")]
    public partial class TBL_JOB_REQUEST_MESSAGE
    {
        [Key]
        public int JOBREQUEST_MESSAGEID { get; set; }

        public int JOBREQUESTID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string MESSAGE { get; set; }

        public int STAFFID { get; set; }

        public DateTime DATE_TIME_SENT { get; set; }

        public virtual TBL_JOB_REQUEST TBL_JOB_REQUEST { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
