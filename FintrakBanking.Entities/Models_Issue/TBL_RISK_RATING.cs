namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RISK_RATING")]
    public partial class TBL_RISK_RATING
    {
        [Key]
        public int RISKRATINGID { get; set; }

        [StringLength(10)]
        public string RATES { get; set; }

        public decimal? MAXRANGE { get; set; }

        public decimal? MINRANGE { get; set; }

        public decimal? ADVICEDRATE { get; set; }

        [StringLength(10)]
        public string RATESDESCRIPTION { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
