namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_City")]
    public partial class tbl_City
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_City()
        {
            tbl_Branch = new HashSet<tbl_Branch>();
<<<<<<< HEAD
            tbl_Collateral_Locations = new HashSet<tbl_Collateral_Locations>();
            tbl_Collateral_Property = new HashSet<tbl_Collateral_Property>();
=======
            tbl_Collateral_Immovable_Property = new HashSet<tbl_Collateral_Immovable_Property>();
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba
            tbl_Staff = new HashSet<tbl_Staff>();
            tbl_Temp_Staff = new HashSet<tbl_Temp_Staff>();
        }

        [Key]
        public int CityId { get; set; }

        [Required]
        [StringLength(150)]
        public string CityName { get; set; }

        public int StateId { get; set; }

        public short CityClassId { get; set; }

        public bool AllowedForCollateral { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Branch> tbl_Branch { get; set; }

        public virtual tbl_State tbl_State { get; set; }

        public virtual tbl_City_Class tbl_City_Class { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
<<<<<<< HEAD
        public virtual ICollection<tbl_Collateral_Locations> tbl_Collateral_Locations { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Collateral_Property> tbl_Collateral_Property { get; set; }
=======
        public virtual ICollection<tbl_Collateral_Immovable_Property> tbl_Collateral_Immovable_Property { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Staff> tbl_Staff { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Temp_Staff> tbl_Temp_Staff { get; set; }
    }
}
