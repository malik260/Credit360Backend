namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Transaction_Type")]
    public partial class tbl_Loan_Transaction_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Transaction_Type()
        {
            tbl_Daily_Accural = new HashSet<tbl_Daily_Accural>();
            tbl_Loan_Force_Debit = new HashSet<tbl_Loan_Force_Debit>();
            tbl_Loan_Past_Due = new HashSet<tbl_Loan_Past_Due>();
        }

        [Key]
        public byte TransactionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Daily_Accural> tbl_Daily_Accural { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Force_Debit> tbl_Loan_Force_Debit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Past_Due> tbl_Loan_Past_Due { get; set; }
    }
}
