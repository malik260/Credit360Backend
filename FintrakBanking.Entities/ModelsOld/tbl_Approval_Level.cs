namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_Level")]
    public partial class tbl_Approval_Level
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Approval_Level()
        {
            tbl_Approval_Level_Staff = new HashSet<tbl_Approval_Level_Staff>();
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
            tbl_Approval_Trail1 = new HashSet<tbl_Approval_Trail>();
            tbl_Credit_Template = new HashSet<tbl_Credit_Template>();
            tbl_Checklist_Definition = new HashSet<tbl_Checklist_Definition>();
        }

        [Key]
        public int ApprovalLevelId { get; set; }

        [Required]
        [StringLength(150)]
        public string LevelName { get; set; }

        public int GroupOperationMappingId { get; set; }

        public int Position { get; set; }

        public int? Tenor { get; set; }

        public short? TenorModeId { get; set; }

        [Column(TypeName = "money")]
        public decimal MinimumAmount { get; set; }

        public int NumberOfUsers { get; set; }

        public int NumberOfApprovals { get; set; }

        public int SLAInterval { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        public bool IsActive { get; set; }

        public bool CanEdit { get; set; }

        public bool CanDoRiskAssessment { get; set; }

        public bool CanRecieveAdjustment { get; set; }

        public bool CanRecieveEmail { get; set; }

        public bool CanRecieveSMS { get; set; }

        public bool HasChecklist { get; set; }

        public bool CanPerformFinancialAnalysis { get; set; }

        public bool RequireAuthorisation { get; set; }

        public bool CanOverideAuthorisation { get; set; }

        public bool RouteViaStaffOrganogram { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Approval_Group_Mapping tbl_Approval_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Level_Staff> tbl_Approval_Level_Staff { get; set; }

        public virtual tbl_Tenor_Mode tbl_Tenor_Mode { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Credit_Template> tbl_Credit_Template { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Checklist_Definition> tbl_Checklist_Definition { get; set; }
    }
}
