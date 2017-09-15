namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Group")]
    public partial class tbl_Customer_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_Group()
        {
            tbl_Customer_Group_Mapping = new HashSet<tbl_Customer_Group_Mapping>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan_Archive = new HashSet<tbl_Loan_Archive>();
            tbl_Loan_Contingent = new HashSet<tbl_Loan_Contingent>();
            tbl_Loan = new HashSet<tbl_Loan>();
            tbl_Loan_Revolving = new HashSet<tbl_Loan_Revolving>();
        }

        [Key]
        public int CustomerGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string GroupName { get; set; }

        [Required]
        [StringLength(10)]
        public string GroupCode { get; set; }

        [StringLength(1000)]
        public string GroupDescription { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Group_Mapping> tbl_Customer_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Archive> tbl_Loan_Archive { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Contingent> tbl_Loan_Contingent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Revolving> tbl_Loan_Revolving { get; set; }
    }
}
