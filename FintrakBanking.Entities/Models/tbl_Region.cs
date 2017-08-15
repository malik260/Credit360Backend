namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Region")]
    public partial class tbl_Region
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Region()
        {
            tbl_State = new HashSet<tbl_State>();
        }

        [Key]
        public int RegionId { get; set; }

        public int CountryId { get; set; }

        [Required]
        [StringLength(100)]
        public string RegionName { get; set; }

        public virtual tbl_Country tbl_Country { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_State> tbl_State { get; set; }
    }
}
