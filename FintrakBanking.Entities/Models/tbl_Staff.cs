namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Staff")]
    public partial class tbl_Staff
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Staff()
        {
            tbl_Approval_Level_Staff = new HashSet<tbl_Approval_Level_Staff>();
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
            tbl_Approval_Trail1 = new HashSet<tbl_Approval_Trail>();
            tbl_Audit = new HashSet<tbl_Audit>();
            tbl_CASA = new HashSet<tbl_CASA>();
            tbl_CASA1 = new HashSet<tbl_CASA>();
            tbl_Customer = new HashSet<tbl_Customer>();
            tbl_Finance_Transaction = new HashSet<tbl_Finance_Transaction>();
            tbl_Finance_Transaction1 = new HashSet<tbl_Finance_Transaction>();
            tbl_Job_Request = new HashSet<tbl_Job_Request>();
            tbl_Job_Request1 = new HashSet<tbl_Job_Request>();
            tbl_Notification_Log = new HashSet<tbl_Notification_Log>();
            tbl_Profile_User = new HashSet<tbl_Profile_User>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Application1 = new HashSet<tbl_Loan_Application>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan1 = new HashSet<tbl_Loan>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            tbl_Loan_Preliminary_Evaluation1 = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            tbl_Loan_Relationship_Officer_History = new HashSet<tbl_Loan_Relationship_Officer_History>();
        }

        [Key]
        public int StaffId { get; set; }

        [Required]
        [StringLength(50)]
        public string StaffCode { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }

        public int JobTitleId { get; set; }

        public int RankId { get; set; }

        [StringLength(100)]
        public string Phone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(1)]
        public string Gender { get; set; }

        [StringLength(100)]
        public string NameOfNOK { get; set; }

        [StringLength(100)]
        public string PhoneOfNOK { get; set; }

        [StringLength(100)]
        public string EmailOfNOK { get; set; }

        [StringLength(100)]
        public string AddressOfNOK { get; set; }

        [StringLength(1)]
        public string GenderOfNOK { get; set; }

        [StringLength(100)]
        public string NOKRelationShip { get; set; }

        [StringLength(100)]
        public string Comment { get; set; }

        public byte[] Staffsignature { get; set; }

        public short? BranchId { get; set; }

        public int? MISInfoId { get; set; }

        public short? DepartmentId { get; set; }

        public int? StateId { get; set; }

        public int? CityId { get; set; }

        public short CustomerSensitivityLevel { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Level_Staff> tbl_Approval_Level_Staff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Audit> tbl_Audit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA1 { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Finance_Transaction> tbl_Finance_Transaction { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Finance_Transaction> tbl_Finance_Transaction1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Job_Request> tbl_Job_Request { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Job_Request> tbl_Job_Request1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Notification_Log> tbl_Notification_Log { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_User> tbl_Profile_User { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Relationship_Officer_History> tbl_Loan_Relationship_Officer_History { get; set; }

        public virtual tbl_Staff_JobTitle tbl_Staff_JobTitle { get; set; }

        public virtual tbl_Staff_Rank tbl_Staff_Rank { get; set; }
    }
}
