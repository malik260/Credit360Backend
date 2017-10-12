namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Guarantor")]
    public partial class tbl_Loan_Guarantor
    {
        [Key]
        public short LoanGuarantorId { get; set; }

        public int LoanApplicationId { get; set; }

        public short ProductTypeId { get; set; }

        [StringLength(50)]
        public string BVN { get; set; }

        [StringLength(250)]
        public string Firstname { get; set; }

        [StringLength(50)]
        public string Middlename { get; set; }

        [StringLength(50)]
        public string Lastname { get; set; }

        [StringLength(100)]
        public string Relationship { get; set; }

        public int? RelationshipDuration { get; set; }

        [StringLength(50)]
        public string PhoneNumber1 { get; set; }

        [StringLength(50)]
        public string PhoneNumber2 { get; set; }

        [StringLength(50)]
        public string EmailAddress { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Product_Type tbl_Product_Type { get; set; }

        public virtual tbl_Loan_Application tbl_Loan_Application { get; set; }
    }
}
