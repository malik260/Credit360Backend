namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_RISK_ASSESSMENT_RESULT")]
    public partial class TBL_RISK_ASSESSMENT_RESULT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ASSESSMENTRESULTID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int RISKASSESSMENTTITLEID { get; set; }

        [StringLength(50)]
        public string CREDITRATING { get; set; }

        public decimal TOTALSCORE { get; set; }

        public int COMPANYID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }
    }
}
