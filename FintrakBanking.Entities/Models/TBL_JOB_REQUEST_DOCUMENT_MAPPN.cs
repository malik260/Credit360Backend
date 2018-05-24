namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_REQUEST_DOCUMENT_MAPPN")]
    public partial class TBL_JOB_REQUEST_DOCUMENT_MAPPN
    {
        [Key]
        public int JOBREQUESTDOCUMENTID { get; set; }

        public int JOBREQUESTID { get; set; }

        public int DOCUMENTID { get; set; }

        public virtual TBL_JOB_REQUEST TBL_JOB_REQUEST { get; set; }
    }
}
