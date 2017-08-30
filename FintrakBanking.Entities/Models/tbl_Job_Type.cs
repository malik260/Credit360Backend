namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Job_Type")]
    public partial class tbl_Job_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Job_Type()
        {
            tbl_Job_Request = new HashSet<tbl_Job_Request>();
        }

        [Key]
        public short JobTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string JobTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Job_Request> tbl_Job_Request { get; set; }
    }
}
