namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CONDITION_PRECEDENT")]
    public partial class TBL_CONDITION_PRECEDENT
    {
        [Key]
        public int CONDITIONID { get; set; }

        [Required]
        [StringLength(1000)]
        public string CONDITION { get; set; }

        public bool ISEXTERNAL { get; set; }

        public bool CORPORATE { get; set; }

        public bool RETAIL { get; set; }

        public int? PRODUCTID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
