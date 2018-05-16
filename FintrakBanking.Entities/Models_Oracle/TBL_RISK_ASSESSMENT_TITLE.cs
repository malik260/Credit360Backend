namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RISK_ASSESSMENT_TITLE")]
    public partial class TBL_RISK_ASSESSMENT_TITLE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_RISK_ASSESSMENT_TITLE()
        {
            TBL_RISK_ASSESSMENT = new HashSet<TBL_RISK_ASSESSMENT>();
            TBL_RISK_ASSESSMENT_INDEX = new HashSet<TBL_RISK_ASSESSMENT_INDEX>();
        }

        [Key]
        public int RISKASSESSMENTTITLEID { get; set; }

        [Required]
        [StringLength(250)]
        public string RISKTITLE { get; set; }

        public int? PRODUCTID { get; set; }

        public int RISKTYPEID { get; set; }

        public int COMPANYID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_RISK_ASSESSMENT> TBL_RISK_ASSESSMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
    }
}
