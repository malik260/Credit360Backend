namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Comment")]
    public partial class tbl_Loan_Comment
    {
        [Key]
        public short LoanCommentId { get; set; }

        public int? LoanId { get; set; }

        [StringLength(20)]
        public string CommentType { get; set; }

        [StringLength(250)]
        public string Comment { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }
    }
}
