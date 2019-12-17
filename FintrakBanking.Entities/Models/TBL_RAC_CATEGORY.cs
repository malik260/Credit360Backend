namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RAC_CATEGORY")]
    public partial class TBL_RAC_CATEGORY
    {
        [Key]
        public int RACCATEGORYID { get; set; }

        [Required]
        //[StringLength(200)]
        public string CATEGORYNAME { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

    }
}
        /*

        public virtual DbSet<TBL_RAC_CATEGORY> TBL_RAC_CATEGORY { get; set; }

        */
