namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Group_RelationshipType")]
    public partial class tbl_Customer_Group_RelationshipType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_Group_RelationshipType()
        {
            tbl_Customer_Group_Mapping = new HashSet<tbl_Customer_Group_Mapping>();
        }

        [Key]
        public short RelationshipTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string RelationshipTypeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_Group_Mapping> tbl_Customer_Group_Mapping { get; set; }

        public virtual tbl_Customer_Group_RelationshipType tbl_Customer_Group_RelationshipType1 { get; set; }

        public virtual tbl_Customer_Group_RelationshipType tbl_Customer_Group_RelationshipType2 { get; set; }
    }
}
