namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_FS_Caption")]
    public partial class tbl_Customer_FS_Caption
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_FS_Caption()
        {
            tbl_Customer_FS_Caption_Detail = new HashSet<tbl_Customer_FS_Caption_Detail>();
            tbl_Customer_FS_Ratio_Detail = new HashSet<tbl_Customer_FS_Ratio_Detail>();
            tbl_Customer_FS_Caption1 = new HashSet<tbl_Customer_FS_Caption>();
        }

        [Key]
        public int FSCaptionId { get; set; }

        [Required]
        [StringLength(50)]
        public string FSCaptionCode { get; set; }

        [Required]
        [StringLength(200)]
        public string FSCaptionName { get; set; }

        public short FSCaptionGroupId { get; set; }

        public int? ParentIdFSCaptionId { get; set; }

        public short? AccountCategoryId { get; set; }

        public short FSTypeId { get; set; }

        public int Position { get; set; }

        [StringLength(50)]
        public string RefNote { get; set; }

        public bool IsTotalLine { get; set; }

        [StringLength(50)]
        public string ReportColour { get; set; }

        public double Multiplier { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption_Detail> tbl_Customer_FS_Caption_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Ratio_Detail> tbl_Customer_FS_Ratio_Detail { get; set; }

        public virtual tbl_Account_Category tbl_Account_Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption> tbl_Customer_FS_Caption1 { get; set; }

        public virtual tbl_Customer_FS_Caption tbl_Customer_FS_Caption2 { get; set; }

        public virtual tbl_Customer_FS_Caption_Group tbl_Customer_FS_Caption_Group { get; set; }

        public virtual tbl_Financial_Statement_Type tbl_Financial_Statement_Type { get; set; }
    }
}
