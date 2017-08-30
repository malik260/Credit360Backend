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

        public int? CustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanReferenceNumber { get; set; }

        public short ProductId { get; set; }

        [Required]
        [StringLength(250)]
        public string FullName { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber1 { get; set; }

        [StringLength(50)]
        public string PhoneNumber2 { get; set; }

        [Required]
        [StringLength(100)]
        public string Relationship { get; set; }

        public int RelationshipDuration { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Guarantor_Document> tbl_Guarantor_Document { get; set; }
    }
}
