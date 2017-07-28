namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Frequency_Type")]
    public partial class tbl_Frequency_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Frequency_Type()
        {
            tbl_Limit_Detail = new HashSet<tbl_Limit_Detail>();
            tbl_Loan_Covenant_Detail = new HashSet<tbl_Loan_Covenant_Detail>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan1 = new HashSet<tbl_Loan>();
            tbl_Loan2 = new HashSet<tbl_Loan>();
            tbl_Loan3 = new HashSet<tbl_Loan>();
        }

        [Key]
        public short FrequencyTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string Mode { get; set; }

        public double Value { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public bool? IsVisible { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Limit_Detail> tbl_Limit_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Covenant_Detail> tbl_Loan_Covenant_Detail { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan3 { get; set; }
    }
}
