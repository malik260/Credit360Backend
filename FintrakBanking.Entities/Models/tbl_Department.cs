namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Department")]
    public partial class tbl_Department
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Department()
        {
            tbl_Job_Request = new HashSet<tbl_Job_Request>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        public short DepartmentId { get; set; }

        public short? BranchId { get; set; }

        [Required]
        [StringLength(50)]
        public string DepartmentCode { get; set; }

        [StringLength(50)]
        public string DepartmentName { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        public int? CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool? Deleted { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Job_Request> tbl_Job_Request { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
