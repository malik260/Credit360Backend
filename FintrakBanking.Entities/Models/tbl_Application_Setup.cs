namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_APPLICATION_SETUP")]
    public partial class TBL_APPLICATION_SETUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short APPLICATIONSETUPID { get; set; }

        [StringLength(500)]
        public string REPORTPATH { get; set; }

        public bool USEACTIVEDIRECTORY { get; set; }

        [StringLength(100)]
        public string ACTIVEDIRECTORYDOMAIN { get; set; }
    }
}
