namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LANGUAGE")]
    public partial class TBL_LANGUAGE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LANGUAGE()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LANGUAGEID { get; set; }

        [Required]
        [StringLength(30)]
        public string LANGUAGECODE { get; set; }

        [Required]
        [StringLength(50)]
        public string LANGUAGENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }
    }
}
