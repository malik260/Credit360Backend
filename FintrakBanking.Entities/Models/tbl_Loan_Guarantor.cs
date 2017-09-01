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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Guarantor()
        {
            tbl_Guarantor_Document = new HashSet<tbl_Guarantor_Document>();
        }

        [Key]
        public short LoanGuarantorId { get; set; }

        public int LoanId { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Guarantor_Document> tbl_Guarantor_Document { get; set; }

        public virtual tbl_Loan tbl_Loan { get; set; }
    }
}
