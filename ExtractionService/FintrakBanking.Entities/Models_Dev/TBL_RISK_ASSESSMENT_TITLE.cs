namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_RISK_ASSESSMENT_TITLE")]
    public partial class TBL_RISK_ASSESSMENT_TITLE
    {
        public TBL_RISK_ASSESSMENT_TITLE()
        {
            TBL_RISK_ASSESSMENT = new HashSet<TBL_RISK_ASSESSMENT>();
            TBL_RISK_ASSESSMENT_INDEX = new HashSet<TBL_RISK_ASSESSMENT_INDEX>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RISKASSESSMENTTITLEID { get; set; }

        [Required]
        [StringLength(250)]
        public string RISKTITLE { get; set; }

        public short? PRODUCTID { get; set; }

        public int RISKTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public virtual ICollection<TBL_RISK_ASSESSMENT> TBL_RISK_ASSESSMENT { get; set; }

        public virtual ICollection<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
    }
}
