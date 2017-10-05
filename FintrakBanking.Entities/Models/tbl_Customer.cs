namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer")]
    public partial class tbl_Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer()
        {
            tbl_CASA = new HashSet<tbl_CASA>();
            tbl_Custom_Fields = new HashSet<tbl_Custom_Fields>();
            tbl_Custom_Fields_Data = new HashSet<tbl_Custom_Fields_Data>();
            tbl_Custom_Fields_Data1 = new HashSet<tbl_Custom_Fields_Data>();
            tbl_Customer_Custom_Field = new HashSet<tbl_Customer_Custom_Field>();
            tbl_Customer_Group_Mapping = new HashSet<tbl_Customer_Group_Mapping>();
            tbl_Customer_Account_KYC_Item = new HashSet<tbl_Customer_Account_KYC_Item>();
            tbl_Customer_Edit_History = new HashSet<tbl_Customer_Edit_History>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
            tbl_Temp_Customer_Group_Mapping = new HashSet<tbl_Temp_Customer_Group_Mapping>();
            tbl_Collateral_Customer = new HashSet<tbl_Collateral_Customer>();
            tbl_Temp_Collateral_Customer = new HashSet<tbl_Temp_Collateral_Customer>();
            tbl_Customer_Blacklist = new HashSet<tbl_Customer_Blacklist>();
            tbl_Customer_BVN = new HashSet<tbl_Customer_BVN>();
            tbl_Customer_CompanyInfomation = new HashSet<tbl_Customer_CompanyInfomation>();
            tbl_Customer_EmploymentHistory = new HashSet<tbl_Customer_EmploymentHistory>();
            tbl_Customer_FS_Caption_Detail = new HashSet<tbl_Customer_FS_Caption_Detail>();
            tbl_Customer_Guardian = new HashSet<tbl_Customer_Guardian>();
            tbl_Customer_Identification = new HashSet<tbl_Customer_Identification>();
            tbl_Customer_NextOfKin = new HashSet<tbl_Customer_NextOfKin>();
            tbl_Customer_PhoneContact = new HashSet<tbl_Customer_PhoneContact>();
            tbl_Customer_Address = new HashSet<tbl_Customer_Address>();
            tbl_Customer_Client_Supplier = new HashSet<tbl_Customer_Client_Supplier>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
        }

        [Key]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomerCode { get; set; }

        public short BranchId { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string FirstName { get; set; }

        [StringLength(200)]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(200)]
        public string LastName { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Column(TypeName = "date")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(200)]
        public string PlaceOfBirth { get; set; }

        [Required]
        [StringLength(20)]
        public string Nationality { get; set; }

        public int? MaritalStatus { get; set; }

        [StringLength(100)]
        public string EmailAddress { get; set; }

        [StringLength(200)]
        public string MaidenName { get; set; }

        [StringLength(200)]
        public string Spouse { get; set; }

        [StringLength(200)]
        public string FirstChildName { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ChildDateOfBirth { get; set; }

        [StringLength(100)]
        public string Occupation { get; set; }

        public short? CustomerTypeId { get; set; }

        public int? RelationshipOfficerId { get; set; }

        public bool IsPoliticallyExposed { get; set; }

        public bool IsInvestmentGrade { get; set; }

        public bool IsRealatedParty { get; set; }

        [StringLength(200)]
        public string MISCode { get; set; }

        [StringLength(200)]
        public string MISStaff { get; set; }

        public int? ApprovalStatus { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateActedOn { get; set; }

        [StringLength(150)]
        public string ActedOnBy { get; set; }

        public bool AccountCreationComplete { get; set; }

        public bool CreationMailSent { get; set; }

        public short CustomerSensitivityLevelId { get; set; }

        public short SubSectorId { get; set; }

        public short? FSCaptionGroupId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [StringLength(50)]
        public string TaxNumber { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Custom_Fields> tbl_Custom_Fields { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Custom_Fields_Data> tbl_Custom_Fields_Data { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Custom_Fields_Data> tbl_Custom_Fields_Data1 { get; set; }

        public virtual tbl_Customer_Type tbl_Customer_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Custom_Field> tbl_Customer_Custom_Field { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Group_Mapping> tbl_Customer_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Account_KYC_Item> tbl_Customer_Account_KYC_Item { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Edit_History> tbl_Customer_Edit_History { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Customer_Group_Mapping> tbl_Temp_Customer_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Customer> tbl_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Collateral_Customer> tbl_Temp_Collateral_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Blacklist> tbl_Customer_Blacklist { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_BVN> tbl_Customer_BVN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_CompanyInfomation> tbl_Customer_CompanyInfomation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_EmploymentHistory> tbl_Customer_EmploymentHistory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption_Detail> tbl_Customer_FS_Caption_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Guardian> tbl_Customer_Guardian { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Identification> tbl_Customer_Identification { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_NextOfKin> tbl_Customer_NextOfKin { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_PhoneContact> tbl_Customer_PhoneContact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Address> tbl_Customer_Address { get; set; }

        public virtual tbl_Customer_FS_Caption_Group tbl_Customer_FS_Caption_Group { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Client_Supplier> tbl_Customer_Client_Supplier { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }
    }
}
