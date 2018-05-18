namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COMPLIANCE_TIMELINE")]
    public partial class TBL_COMPLIANCE_TIMELINE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TIMELINEID { get; set; }

        [Required]
        [StringLength(1000)]
        public string TIMELINE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
