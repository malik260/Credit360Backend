namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Source_Application")]
    public partial class tbl_Source_Application
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ApplicationId { get; set; }

        [StringLength(150)]
        public string ApplicationName { get; set; }
    }
}
