namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan")]
    public partial class tbl_Loan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan()
        {
            tbl_Loan_Amortization_Schedule = new HashSet<tbl_Loan_Amortization_Schedule>();
            tbl_Loan_Camsol = new HashSet<tbl_Loan_Camsol>();
            tbl_Loan_Collateral_Mapping = new HashSet<tbl_Loan_Collateral_Mapping>();
            tbl_Loan_Covenant_Detail = new HashSet<tbl_Loan_Covenant_Detail>();
            tbl_Loan_Fee = new HashSet<tbl_Loan_Fee>();
            tbl_Loan_Guarantor = new HashSet<tbl_Loan_Guarantor>();
            tbl_Loan_Schedule_Daily = new HashSet<tbl_Loan_Schedule_Daily>();
            tbl_Loan_Schedule_Irregular_Input = new HashSet<tbl_Loan_Schedule_Irregular_Input>();
            tbl_Loan_Schedule_Periodic = new HashSet<tbl_Loan_Schedule_Periodic>();
        }

        [Key]
        public int LoanId { get; set; }

        public int CustomerId { get; set; }

        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public int CasaAccountId { get; set; }

        public short BranchId { get; set; }

        public short CurrencyId { get; set; }

        public double ExchangeRate { get; set; }

        public int LoanApplicationId { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanReferenceNumber { get; set; }

        public int Tenor { get; set; }

        public short SubSectorId { get; set; }

        public short? PrincipalFrequencyTypeId { get; set; }

        public short? InterestFrequencyTypeId { get; set; }

        public int PrincipalNumberOfInstallment { get; set; }

        public int InterestNumberOfInstallment { get; set; }

        public int RelationshipOfficerId { get; set; }

        public int RelationshipManagerId { get; set; }

        [StringLength(50)]
        public string MISCode { get; set; }

        [StringLength(50)]
        public string TeamMISCode { get; set; }

        public double InterestRate { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime MaturityDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime BookingDate { get; set; }

        [Column(TypeName = "money")]
        public decimal PrincipalAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal ApprovedAmount { get; set; }

        public int PrincipalInstallmentLeft { get; set; }

        public int InterestInstallmentLeft { get; set; }

        public int ApprovalStatusId { get; set; }

        [StringLength(50)]
        public string ApprovedBy { get; set; }

        [StringLength(500)]
        public string ApproverComment { get; set; }

        public DateTime? DateApproved { get; set; }

        public short LoanStatusId { get; set; }

        public short ScheduleTypeId { get; set; }

        public short ScheduleDayCountConventionId { get; set; }

        public short ScheduleDayInterestTypeId { get; set; }

        public bool IsDisbursed { get; set; }

        [StringLength(50)]
        public string DisbursedBy { get; set; }

        [StringLength(500)]
        public string DisburserComment { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DisburseDate { get; set; }

        public int? OperationId { get; set; }

        public int? CustomerGroupId { get; set; }

        public short LoanTypeId { get; set; }

        [StringLength(50)]
        public string TrancheBatchCode { get; set; }

        [Column(TypeName = "money")]
        public decimal EquityContribution { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FirstPrincipalPaymentDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? FirstInterestPaymentDate { get; set; }

        [Column(TypeName = "money")]
        public decimal OutstandingPrincipal { get; set; }

        [Column(TypeName = "money")]
        public decimal OutstandingInterest { get; set; }

        public int? PrincipalAdditionCount { get; set; }

        public int? PrincipalReductionCount { get; set; }

        public bool FixedPrincipal { get; set; }

        public bool ProfileLoan { get; set; }

        public bool DischargeLetter { get; set; }

        public bool SuspendInterest { get; set; }

        public bool? IsScheduledPrepayment { get; set; }

        public bool AllowForceDebitRepayment { get; set; }

        [Column(TypeName = "money")]
        public decimal? ScheduledPrepaymentAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ScheduledPrepaymentDate { get; set; }

        public short? ScheduledPrepaymentFrequencyTypeId { get; set; }

        public short CustomerSensitivityLevelId { get; set; }

        public int? InternalPrudentialGuidelineStatusId { get; set; }

        public int? ExternalPrudentialGuidelineStatusId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_CASA tbl_CASA { get; set; }

        public virtual tbl_CASA tbl_CASA1 { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_Group tbl_Customer_Group { get; set; }

        public virtual tbl_Customer_Sensitivity_Level tbl_Customer_Sensitivity_Level { get; set; }

        public virtual tbl_Day_Count_Convention tbl_Day_Count_Convention { get; set; }

        public virtual tbl_Day_Interest_Type tbl_Day_Interest_Type { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type1 { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type2 { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Amortization_Schedule> tbl_Loan_Amortization_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Camsol> tbl_Loan_Camsol { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Collateral_Mapping> tbl_Loan_Collateral_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Covenant_Detail> tbl_Loan_Covenant_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Fee> tbl_Loan_Fee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Guarantor> tbl_Loan_Guarantor { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Schedule_Daily> tbl_Loan_Schedule_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Schedule_Irregular_Input> tbl_Loan_Schedule_Irregular_Input { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Schedule_Periodic> tbl_Loan_Schedule_Periodic { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }

        public virtual tbl_Loan_PrudentialGuideline tbl_Loan_PrudentialGuideline { get; set; }

        public virtual tbl_Loan_PrudentialGuideline tbl_Loan_PrudentialGuideline1 { get; set; }

        public virtual tbl_Loan_Schedule_Type tbl_Loan_Schedule_Type { get; set; }

        public virtual tbl_Loan_Status tbl_Loan_Status { get; set; }

        public virtual tbl_Loan_Type tbl_Loan_Type { get; set; }
    }
}
