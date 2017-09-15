namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Status")]
    public partial class tbl_Loan_Status
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Status()
        {
            tbl_Loan = new HashSet<tbl_Loan>();
        }

        [Key]
        public short LoanStatusId { get; set; }

        public short? OperationId { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        public virtual tbl_Loan_Operation tbl_Loan_Operation { get; set; }
    }
}
