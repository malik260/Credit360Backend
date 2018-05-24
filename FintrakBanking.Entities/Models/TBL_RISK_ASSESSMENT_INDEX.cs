namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_RISK_ASSESSMENT_INDEX")]
    public partial class TBL_RISK_ASSESSMENT_INDEX
    {
        [Key]
        public int RISKID { get; set; }

        [Required]
        [StringLength(250)]
        public string NAME { get; set; }

        [Required]
        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        public decimal WEIGHT { get; set; }

        public int? ITEMLEVEL { get; set; }

        public int RISKASSESSMENTTITLEID { get; set; }

        public int? PARENTID { get; set; }

        public int COMPANYID { get; set; }

        public short INDEXTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_RISK_ASSESSMENT_INDEX_TYPE TBL_RISK_ASSESSMENT_INDEX_TYPE { get; set; }

        public virtual TBL_RISK_ASSESSMENT_TITLE TBL_RISK_ASSESSMENT_TITLE { get; set; }
    }
}
