namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CHARGE_FEE_DETAIL")]
    public partial class TBL_CHARGE_FEE_DETAIL
    {
        [Key]
        public int CHARGEFEEDETAILID { get; set; }

        [Required]
        [StringLength(200)]
        public string DESCRIPTION { get; set; }

        public int CHARGEFEEID { get; set; }

        public int? GLACCOUNTID1 { get; set; }

        public int? GLACCOUNTID2 { get; set; }

        public short DETAILTYPEID { get; set; }

        public short POSTINGTYPEID { get; set; }

        public double VALUE { get; set; }

        public short FEETYPEID { get; set; }

        public bool REQUIREAMORTISATION { get; set; }

        public short POSTINGGROUP { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_CHARGE_FEE_DETAIL_TYPE TBL_CHARGE_FEE_DETAIL_TYPE { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT1 { get; set; }

        public virtual TBL_FEE_TYPE TBL_FEE_TYPE { get; set; }

        public virtual TBL_POSTING_TYPE TBL_POSTING_TYPE { get; set; }
    }
}
