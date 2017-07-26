namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Identification")]
    public partial class tbl_Customer_Identification
    {
        [Key]
        public int IdentificationId { get; set; }

        public int CustomerId { get; set; }

        [StringLength(25)]
        public string IdentificationNo { get; set; }

        public int? IdentificationModeId { get; set; }

        [StringLength(200)]
        public string IssuePlace { get; set; }

        [StringLength(200)]
        public string IssueAuthority { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_IdentificationModeType tbl_Customer_IdentificationModeType { get; set; }
    }
}
