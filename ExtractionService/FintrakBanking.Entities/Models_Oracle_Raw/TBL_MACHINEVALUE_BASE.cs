namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_MACHINEVALUE_BASE")]
    public partial class TBL_MACHINEVALUE_BASE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MACHINEVALUEBASEID { get; set; }

        [Required]
        [StringLength(100)]
        public string MACHINEVALUEBASENAME { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
