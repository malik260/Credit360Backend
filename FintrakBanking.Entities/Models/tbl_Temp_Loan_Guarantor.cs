namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Loan_Guarantor")]
    public partial class tbl_Temp_Loan_Guarantor
    {
        [Key]
        public short LoanGuarantorId { get; set; }

        public int LoanApplicationId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(350)]
        public string FullName { get; set; }

        [Required]
        [StringLength(16)]
        public string PhoneNumber1 { get; set; }

        [StringLength(16)]
        public string PhoneNumber2 { get; set; }

        [Required]
        [StringLength(50)]
        public string Relationship { get; set; }

        public short RelationshipDuration { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }
    }
}
