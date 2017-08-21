namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Custom_HostPage")]
    public partial class tbl_Custom_HostPage
    {
        [Key]
        public int HostPageId { get; set; }

        [Required]
        [StringLength(50)]
        public string HostPage { get; set; }

        public int ParentHostPageId { get; set; }

        public virtual tbl_Custom_HostPage tbl_Custom_HostPage1 { get; set; }

        public virtual tbl_Custom_HostPage tbl_Custom_HostPage2 { get; set; }
    }
}
