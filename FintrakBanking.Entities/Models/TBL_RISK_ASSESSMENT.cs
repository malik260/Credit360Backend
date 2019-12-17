namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RISK_ASSESSMENT")]
    public partial class TBL_RISK_ASSESSMENT
    {
        [Key]
        public int RISKASSESSMENTID { get; set; }

        public int RISKINDEXID { get; set; }

        public int? PARENTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string REFCODE { get; set; }

        public int RISKASSESSMENTTITLEID { get; set; }

        public decimal INDEXSCORE { get; set; }

        public int COMPANYID { get; set; }

        public bool SELECTED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        //public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_RISK_ASSESSMENT_TITLE TBL_RISK_ASSESSMENT_TITLE { get; set; }
        public int? TARGETID { get; set; }
    }
}
