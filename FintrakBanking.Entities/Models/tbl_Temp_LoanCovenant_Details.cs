namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_LoanCovenant_Details")]
    public partial class tbl_Temp_LoanCovenant_Details
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short TempLoanCovenantDetailId { get; set; }

        [StringLength(50)]
        public string CovenantDetail { get; set; }

        public int? LoanApplicationId { get; set; }

        public int? CustomerId { get; set; }

        public short? CovenantTypeId { get; set; }

        public short? FrequencyTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? CovenantAmount { get; set; }

        public DateTime? CovenantDate { get; set; }

        public int? CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
