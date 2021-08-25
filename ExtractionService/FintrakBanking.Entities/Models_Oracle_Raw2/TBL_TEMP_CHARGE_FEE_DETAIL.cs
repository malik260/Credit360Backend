namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CHARGE_FEE_DETAIL")]
    public partial class TBL_TEMP_CHARGE_FEE_DETAIL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCHARGEFEEDETAILID { get; set; }

        public int? CHARGEFEEDETAILID { get; set; }

        [Required]
        [StringLength(200)]
        public string DESCRIPTION { get; set; }

        public int TEMPCHARGEFEEID { get; set; }

        public int? GLACCOUNTID1 { get; set; }

        public int? GLACCOUNTID2 { get; set; }

        public int DETAILTYPEID { get; set; }

        public int POSTINGTYPEID { get; set; }

        public decimal VALUE { get; set; }

        public int FEETYPEID { get; set; }

        public int REQUIREAMORTISATION { get; set; }

        public int POSTINGGROUP { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT1 { get; set; }

        public virtual TBL_FEE_TYPE TBL_FEE_TYPE { get; set; }

        public virtual TBL_POSTING_TYPE TBL_POSTING_TYPE { get; set; }

        public virtual TBL_TEMP_CHARGE_FEE TBL_TEMP_CHARGE_FEE { get; set; }
    }
}
