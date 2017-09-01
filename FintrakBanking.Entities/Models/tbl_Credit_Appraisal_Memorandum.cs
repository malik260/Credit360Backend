namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Credit_Appraisal_Memorandum")]
    public partial class tbl_Credit_Appraisal_Memorandum
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Credit_Appraisal_Memorandum()
        {
            tbl_Credit_Appraisal_Memorandum_Loan_Detail = new HashSet<tbl_Credit_Appraisal_Memorandum_Loan_Detail>();
        }

        [Key]
        public int AppraisalMemorandumId { get; set; }

        public int LoanApplicationId { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(50)]
        public string CAMRef { get; set; }

        public bool IsCompleted { get; set; }

        public bool RiskRated { get; set; }

        public string CAMDocumentation { get; set; }

        [Column(TypeName = "xml")]
        public string LoanDetails { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Credit_Appraisal_Memorandum_Loan_Detail> tbl_Credit_Appraisal_Memorandum_Loan_Detail { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
