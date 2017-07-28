namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Limit")]
    public partial class tbl_Limit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Limit()
        {
            tbl_Limit_Detail = new HashSet<tbl_Limit_Detail>();
        }

        [Key]
        public int LimitId { get; set; }

        [Required]
        [StringLength(200)]
        public string LimitName { get; set; }

        public int CompanyId { get; set; }

        public int LimitValueTypeId { get; set; }

        public int LimitMetricId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Limit_Detail> tbl_Limit_Detail { get; set; }

        public virtual tbl_Limit_Metric tbl_Limit_Metric { get; set; }

        public virtual tbl_Limit_Value_Type tbl_Limit_Value_Type { get; set; }
    }
}
