namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Finance_Transaction")]
    public partial class tbl_Finance_Transaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        [StringLength(50)]
        public string BatchCode { get; set; }

        public int GLAccountId { get; set; }

        public int OperationId { get; set; }

        [Required]
        [StringLength(50)]
        public string SourceReferenceNumber { get; set; }

        public int? CasaAccountId { get; set; }

        [Column(TypeName = "money")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal CreditAmount { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "date")]
        public DateTime ValueDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime TransactionDate { get; set; }

        public int CompanyId { get; set; }

        public short SourceBranchId { get; set; }

        public short DestinationBranchId { get; set; }

        public short CurrencyId { get; set; }

        public double CurrencyRate { get; set; }

        public DateTime PostedDateTime { get; set; }

        public bool IsApproved { get; set; }

        public int PostedBy { get; set; }

        public int ApprovedBy { get; set; }

        [Column(TypeName = "date")]
        public DateTime ApprovedDate { get; set; }

        public DateTime ApprovedDateTime { get; set; }

        public short SourceApplicationId { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_Branch tbl_Branch1 { get; set; }

        public virtual tbl_CASA tbl_CASA { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }

        public virtual tbl_Staff tbl_Staff1 { get; set; }
    }
}
