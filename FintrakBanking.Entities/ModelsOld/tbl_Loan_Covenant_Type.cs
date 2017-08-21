namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Loan_Covenant_Type")]
    public partial class tbl_Loan_Covenant_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Loan_Covenant_Type()
        {
            tbl_Loan_Covenant_Detail = new HashSet<tbl_Loan_Covenant_Detail>();
        }

        [Key]
        public short CovenantTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CovenantTypeName { get; set; }

        public bool RequireAmount { get; set; }

        public bool RequireFrequency { get; set; }

        public bool IsCleanupCycle { get; set; }

        public int CompanyId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Covenant_Detail> tbl_Loan_Covenant_Detail { get; set; }
    }
}
