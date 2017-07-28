namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_ErrorLog")]
    public partial class tbl_ErrorLog
    {
        [Key]
        public int ErrorLogId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string ErrorType { get; set; }

        [Required]
        [StringLength(100)]
        public string ErrorSource { get; set; }

        [Required]
        [StringLength(500)]
        public string ErrorMessage { get; set; }

        [Required]
        [StringLength(250)]
        public string APIEndpoint { get; set; }

        [StringLength(250)]
        public string ErrorPath { get; set; }

        public int? StatusCode { get; set; }

        public DateTime TimeUtc { get; set; }

        [Column(TypeName = "ntext")]
        public string AllXml { get; set; }
    }
}
