namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_FS_Ratio_Caption")]
    public partial class tbl_Customer_FS_Ratio_Caption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_FS_Ratio_Caption()
        {
            tbl_Customer_FS_Ratio_Detail = new HashSet<tbl_Customer_FS_Ratio_Detail>();
        }

        [Key]
        public short RatioCaptionId { get; set; }

        [Required]
        [StringLength(200)]
        public string RatioCaption { get; set; }

        public int CompanyId { get; set; }

        public bool Annualised { get; set; }

        public int Position { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Ratio_Detail> tbl_Customer_FS_Ratio_Detail { get; set; }
    }
}
