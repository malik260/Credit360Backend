namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_State")]
    public partial class tbl_Approval_State
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Approval_State()
        {
            tbl_Approval_Trail = new HashSet<tbl_Approval_Trail>();
        }

        [Key]
        public short ApprovalStateId { get; set; }

        [StringLength(50)]
        public string ApprovalState { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Trail> tbl_Approval_Trail { get; set; }
    }
}
