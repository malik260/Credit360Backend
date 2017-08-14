namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Limit_Value_Type")]
    public partial class tbl_Limit_Value_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Limit_Value_Type()
        {
            tbl_Limit = new HashSet<tbl_Limit>();
        }

        [Key]
        public int LimitValueTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string LimitValueTypeName { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Limit> tbl_Limit { get; set; }
    }
}
