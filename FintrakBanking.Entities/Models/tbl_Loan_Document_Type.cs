namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Loan_Document_Type
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LoanDocumentTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; }
    }
}
