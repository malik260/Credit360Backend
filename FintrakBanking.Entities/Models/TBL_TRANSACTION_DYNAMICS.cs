namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_TRANSACTION_DYNAMICS")]
    public partial class TBL_TRANSACTION_DYNAMICS
    {
        [Key]
        public int DYNAMICSID { get; set; }

        [Required]
        [StringLength(1000)]
        public string DYNAMICS { get; set; }

        public short PRODUCTID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
