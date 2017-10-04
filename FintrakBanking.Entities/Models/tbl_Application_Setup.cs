namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Application_Setup")]
    public partial class tbl_Application_Setup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ApplicationSetupId { get; set; }

        [StringLength(500)]
        public string ReportPath { get; set; }
    }
}
