namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

<<<<<<< HEAD:FintrakBanking.Entities/Models/tbl_PropertyValue_Basetype.cs
    [Table("credit.tbl_PropertyValue_Basetype")]
    public partial class tbl_PropertyValue_Basetype
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_PropertyValue_Basetype()
        {
            tbl_Collateral_Property = new HashSet<tbl_Collateral_Property>();
=======
    [Table("credit.tbl_Collateral_Valuebase_Type")]
    public partial class tbl_Collateral_Valuebase_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Collateral_Valuebase_Type()
        {
            tbl_Collateral_Plant_And_Equipment = new HashSet<tbl_Collateral_Plant_And_Equipment>();
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba:FintrakBanking.Entities/Models/tbl_Collateral_ValueBase_Type.cs
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte propertyValueBaseTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string propertyValueBaseTypeName { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
<<<<<<< HEAD:FintrakBanking.Entities/Models/tbl_PropertyValue_Basetype.cs
        public virtual ICollection<tbl_Collateral_Property> tbl_Collateral_Property { get; set; }
=======
        public virtual ICollection<tbl_Collateral_Plant_And_Equipment> tbl_Collateral_Plant_And_Equipment { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba:FintrakBanking.Entities/Models/tbl_Collateral_ValueBase_Type.cs
    }
}
