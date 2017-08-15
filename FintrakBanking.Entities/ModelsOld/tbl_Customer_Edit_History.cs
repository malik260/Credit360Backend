namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Edit_History")]
    public partial class tbl_Customer_Edit_History
    {
        [Key]
        public int CustomerEditHistoryId { get; set; }

        public int? CustomerId { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}
