namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CHARGE_RANGE")]
    public partial class TBL_CHARGE_RANGE
    {
        [Key]
        public int CHARGERANGEID { get; set; }

        //[Column(TypeName = "money")]
        public decimal? MINIMUM { get; set; }

        //[Column(TypeName = "money")]
        public decimal? MAXIMUM { get; set; }

        public bool? MINIMUMANDABOVE { get; set; }

        public bool? MAXIMUMANDBELOW { get; set; }

        public double? RATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        public int CHARGEFEEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }
    }
}
