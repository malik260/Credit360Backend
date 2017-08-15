namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Job_Request_Document_Mapping")]
    public partial class tbl_Job_Request_Document_Mapping
    {
        [Key]
        public int JobRequestDocumentId { get; set; }

        public int JobRequestId { get; set; }

        public int DocumentId { get; set; }

        public virtual tbl_Job_Request tbl_Job_Request { get; set; }
    }
}
