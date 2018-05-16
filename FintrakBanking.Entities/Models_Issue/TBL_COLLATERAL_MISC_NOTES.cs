namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_MISC_NOTES")]
    public partial class TBL_COLLATERAL_MISC_NOTES
    {
        [Key]
        public int MISCELLANEOUSNOTEID { get; set; }

        public int? MISCELLANEOUSID { get; set; }

        [Required]
        [StringLength(50)]
        public string COLUMNNAME { get; set; }

        [StringLength(250)]
        public string COLUMNVALUE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COLLATERAL_MISCELLANEOUS TBL_COLLATERAL_MISCELLANEOUS { get; set; }
    }
}
