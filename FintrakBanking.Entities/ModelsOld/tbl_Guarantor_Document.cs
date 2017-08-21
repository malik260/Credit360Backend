namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Guarantor_Document")]
    public partial class tbl_Guarantor_Document
    {
        [Key]
        public short GuarantorDocumentId { get; set; }

        public short? LoanGuarantorId { get; set; }

        [StringLength(250)]
        public string FilePath { get; set; }

        public virtual tbl_Loan_Guarantor tbl_Loan_Guarantor { get; set; }
    }
}
