namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Branch")]
    public partial class tbl_Branch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Branch()
        {
            tbl_Audit = new HashSet<tbl_Audit>();
            tbl_Chart_Of_Account = new HashSet<tbl_Chart_Of_Account>();
            tbl_Temp_Chart_Of_Account = new HashSet<tbl_Temp_Chart_Of_Account>();
            tbl_Customer = new HashSet<tbl_Customer>();
            tbl_Department = new HashSet<tbl_Department>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
            tbl_CASA = new HashSet<tbl_CASA>();
            tbl_Loan_Application = new HashSet<tbl_Loan_Application>();
            tbl_Loan = new HashSet<tbl_Loan>();
        }

        [Key]
        public short BranchId { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [StringLength(250)]
        public string BranchName { get; set; }

        [Required]
        [StringLength(50)]
        public string BranchCode { get; set; }

        [StringLength(255)]
        public string AddressLine1 { get; set; }

        [StringLength(255)]
        public string AddressLine2 { get; set; }

        [StringLength(2000)]
        public string Comment { get; set; }

        public int? StateId { get; set; }

        public int? CityId { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Audit> tbl_Audit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Chart_Of_Account> tbl_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Chart_Of_Account> tbl_Temp_Chart_Of_Account { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Department> tbl_Department { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_State tbl_State { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan_Application> tbl_Loan_Application { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Loan> tbl_Loan { get; set; }
    }
}
