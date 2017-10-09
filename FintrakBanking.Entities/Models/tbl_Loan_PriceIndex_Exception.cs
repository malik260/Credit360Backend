namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_PriceIndex_Exception")]
    public partial class tbl_Loan_PriceIndex_Exception
    {
        [Key]
        public int LoanPriceExceptionId { get; set; }

        public int LoanId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }
    }
}
