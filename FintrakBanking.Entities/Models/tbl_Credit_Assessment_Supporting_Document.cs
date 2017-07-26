namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Assessment_Supporting_Document")]
    public partial class tbl_Credit_Assessment_Supporting_Document
    {
        [Key]
        public int CreditAssessmentDocumentId { get; set; }

        [Required]
        [StringLength(250)]
        public string DocumentTitle { get; set; }

        [Required]
        [StringLength(250)]
        public string FileName { get; set; }

        [Required]
        public byte[] DocumentData { get; set; }

        [Required]
        [StringLength(10)]
        public string ContentType { get; set; }

        public int AccessmentMemorandumId { get; set; }

        public int CustomerId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
    }
}
