namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_Assessment_Title")]
    public partial class tbl_Risk_Assessment_Title
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Risk_Assessment_Title()
        {
            tbl_Risk_Assessment_Index = new HashSet<tbl_Risk_Assessment_Index>();
        }

        [Key]
        public int RiskAssessmentTitleId { get; set; }

        [Required]
        [StringLength(250)]
        public string RiskTitle { get; set; }

        public int? ProductId { get; set; }

        public int RiskTypeId { get; set; }

        public int CompanyId { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Risk_Assessment_Index> tbl_Risk_Assessment_Index { get; set; }
    }
}
