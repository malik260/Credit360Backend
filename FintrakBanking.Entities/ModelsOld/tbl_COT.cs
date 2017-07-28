namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_COT
    {
        [Key]
        public int COTId { get; set; }

        public int? AccountId { get; set; }

        [Column(TypeName = "money")]
        public decimal? COTAccountAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime? COTDate { get; set; }

        [StringLength(50)]
        public string COTCreatedBy { get; set; }

        public bool? IsCurrent { get; set; }
    }
}
