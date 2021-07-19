namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_MANAGEMENT_TYPE")]
    public partial class TBL_MANAGEMENT_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_MANAGEMENT_TYPE()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
        }

        [Key]
        public short MANAGEMENTTYPEID { get; set; }

        [Required]
        //[StringLength(250)]
        public string NAME { get; set; }

        //[StringLength(250)]
        public string DESCRIPTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }
    }
}
