namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Approval_Group_Mapping")]
    public partial class tbl_Approval_Group_Mapping
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Approval_Group_Mapping()
        {
            tbl_Approval_Level = new HashSet<tbl_Approval_Level>();
        }

        [Key]
        public int GroupOperationMappingId { get; set; }

        public int OperationId { get; set; }

        public int GroupId { get; set; }

        public short? ProductClassId { get; set; }

        public short? ProductId { get; set; }

        public int Position { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Approval_Group tbl_Approval_Group { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }

        public virtual tbl_Product_Class tbl_Product_Class { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Approval_Level> tbl_Approval_Level { get; set; }
    }
}
