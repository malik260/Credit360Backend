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

        public short ReportingCurrencyId { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }
    }
}
