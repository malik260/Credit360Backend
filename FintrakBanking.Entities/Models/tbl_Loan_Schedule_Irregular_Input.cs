namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Schedule_Irregular_Input")]
    public partial class tbl_Loan_Schedule_Irregular_Input
    {
        [Key]
        public int IrregularScheduleInputId { get; set; }

        public int LoanId { get; set; }

        [Column(TypeName = "date")]
        public DateTime PaymentDate { get; set; }

        [Column(TypeName = "money")]
        public decimal PaymentAmount { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
