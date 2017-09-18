namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Covenant_Detail")]
    public partial class tbl_Loan_Covenant_Detail
    {
        [Key]
        public int LoanCovenantDetailId { get; set; }

        [Required]
        [StringLength(2000)]
        public string CovenantDetail { get; set; }

        public int LoanId { get; set; }

        public short ProductTypeId { get; set; }

        public short CovenantTypeId { get; set; }

        public short? FrequencyTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? CovenantAmount { get; set; }

        [Column(TypeName = "date")]
        public DateTime CovenantDate { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Loan_Covenant_Type tbl_Loan_Covenant_Type { get; set; }
    }
}
