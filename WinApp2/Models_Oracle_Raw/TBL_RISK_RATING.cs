namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_RISK_RATING")]
    public partial class TBL_RISK_RATING
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RISKRATINGID { get; set; }

        [StringLength(10)]
        public string RATES { get; set; }

        public decimal? MAXRANGE { get; set; }

        public decimal? MINRANGE { get; set; }

        public decimal? ADVICEDRATE { get; set; }

        [StringLength(10)]
        public string RATESDESCRIPTION { get; set; }

        public int PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
