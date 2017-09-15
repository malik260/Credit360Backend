namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Sub_Sector")]
    public partial class tbl_Sub_Sector
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Sub_Sector()
        {
            tbl_Customer = new HashSet<tbl_Customer>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan_Preliminary_Evaluation = new HashSet<tbl_Loan_Preliminary_Evaluation>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
            tbl_Loan = new HashSet<tbl_Loan>();
        }

        [Key]
        public short SubSectorId { get; set; }

        public short? SectorId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }

        public virtual tbl_Sector tbl_Sector { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Preliminary_Evaluation> tbl_Loan_Preliminary_Evaluation { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }
    }
}
