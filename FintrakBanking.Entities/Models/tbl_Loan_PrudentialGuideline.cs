namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_PrudentialGuideline")]
    public partial class tbl_Loan_PrudentialGuideline
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_PrudentialGuideline()
        {
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan1 = new HashSet<tbl_Loan>();
        }

        [Key]
        public int PrudentialGuidelineStatusId { get; set; }

        [StringLength(250)]
        public string StatusName { get; set; }

        [StringLength(250)]
        public string Classification { get; set; }

        public int? InternalMinimum { get; set; }

        public int? InternalMaximum { get; set; }

        public int? ExternalMinimum { get; set; }

        public int? ExternalMaximum { get; set; }

        [StringLength(500)]
        public string Narration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan1 { get; set; }
    }
}
