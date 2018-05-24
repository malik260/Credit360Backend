namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_RISK_ASSESSMENT_INDEX_TYPE")]
    public partial class TBL_RISK_ASSESSMENT_INDEX_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_RISK_ASSESSMENT_INDEX_TYPE()
        {
            TBL_RISK_ASSESSMENT_INDEX = new HashSet<TBL_RISK_ASSESSMENT_INDEX>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short INDEXTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string INDEXTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
    }
}
