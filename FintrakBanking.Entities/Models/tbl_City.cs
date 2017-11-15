namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CITY")]
    public partial class TBL_CITY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CITY()
        {
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
            TBL_COLLATERAL_IMMOVABLE_PROPERTY = new HashSet<TBL_COLLATERAL_IMMOVABLE_PROPERTY>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY = new HashSet<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        public int CITYID { get; set; }

        [Required]
        [StringLength(150)]
        public string CITYNAME { get; set; }

        public int STATEID { get; set; }

        public short CITYCLASSID { get; set; }

        public bool ALLOWEDFORCOLLATERAL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }

        public virtual TBL_CITY_CLASS TBL_CITY_CLASS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_IMMOVABLE_PROPERTY> TBL_COLLATERAL_IMMOVABLE_PROPERTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY> TBL_TEMP_COLLATERAL_IMMOVABLE_PROPERTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
