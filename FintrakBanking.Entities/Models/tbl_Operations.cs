namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Operations")]
    public partial class tbl_Operations
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Operations()
        {
            tbl_Approval_Group_Mapping = new HashSet<tbl_Approval_Group_Mapping>();
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int OperationId { get; set; }

        [Required]
        [StringLength(150)]
        public string OperationName { get; set; }

        public short OperationTypeId { get; set; }

        public bool TerminateIfDisapproved { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Group_Mapping> tbl_Approval_Group_Mapping { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }

        public virtual tbl_Operations_Type tbl_Operations_Type { get; set; }
    }
}
