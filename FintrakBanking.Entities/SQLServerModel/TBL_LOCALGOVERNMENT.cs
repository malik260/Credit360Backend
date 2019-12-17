namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_LOCALGOVERNMENT")]
    public partial class TBL_LOCALGOVERNMENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOCALGOVERNMENT()
        {
            TBL_CITY = new HashSet<TBL_CITY>();
        }

        [Key]
        public int LOCALGOVERNMENTID { get; set; }

        [Required]
        //[StringLength(200)]
        public string NAME { get; set; }

        public int STATEID { get; set; }

        //[StringLength(50)]
        public string LGACODE { get; set; }

        //[StringLength(50)]
        public string CRMSCODE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CITY> TBL_CITY { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }
    }
}
