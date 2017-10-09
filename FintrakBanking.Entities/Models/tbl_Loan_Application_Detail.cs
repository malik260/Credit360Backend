namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Application_Detail")]
    public partial class tbl_Loan_Application_Detail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Application_Detail()
        {
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
        }

        [Key]
        public int LoanApplicationDetailId { get; set; }

        public int LoanApplicationId { get; set; }

        public int CustomerId { get; set; }

        public short ProposedProductId { get; set; }

        public int ProposedTenor { get; set; }

        public double ProposedInterestRate { get; set; }

        [Column(TypeName = "money")]
        public decimal ProposedAmount { get; set; }

        public short ApprovedProductId { get; set; }

        public int ApprovedTenor { get; set; }

        public double ApprovedInterestRate { get; set; }

        [Column(TypeName = "money")]
        public decimal ApprovedAmount { get; set; }

        public short CurrencyId { get; set; }

        public double ExchangeRate { get; set; }

        public short SubSectorId { get; set; }

        public short StatusId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Product tbl_Product1 { get; set; }

        public virtual tbl_Sub_Sector tbl_Sub_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }

        public virtual tbl_Loan_Application_Detail_Status tbl_Loan_Application_Detail_Status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }
    }
}
