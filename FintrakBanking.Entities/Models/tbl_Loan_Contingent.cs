namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Contingent")]
    public partial class tbl_Loan_Contingent
    {
        [Key]
        public int ContingentLoanId { get; set; }

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

        public short SubSectorId { get; set; }

        public int RelationshipOfficerId { get; set; }

        public int RelationshipManagerId { get; set; }

        [StringLength(50)]
        public string MISCode { get; set; }

        [StringLength(50)]
        public string TeamMISCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime EffectiveDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime MaturityDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime BookingDate { get; set; }

        [Column(TypeName = "money")]
        public decimal ContingentAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal ApprovedAmount { get; set; }

        public int ApprovalStatusId { get; set; }

        [StringLength(50)]
        public string ApprovedBy { get; set; }

        [StringLength(500)]
        public string ApproverComment { get; set; }

        public DateTime? DateApproved { get; set; }

        public short LoanStatusId { get; set; }

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

        public bool DischargeLetter { get; set; }

        public short CustomerSensitivityLevelId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_CASA tbl_CASA { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_Group tbl_Customer_Group { get; set; }

        public virtual tbl_Customer_Sensitivity_Level tbl_Customer_Sensitivity_Level { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }

        public virtual tbl_Loan_Status tbl_Loan_Status { get; set; }

        public virtual tbl_Loan_Type tbl_Loan_Type { get; set; }
    }
}
