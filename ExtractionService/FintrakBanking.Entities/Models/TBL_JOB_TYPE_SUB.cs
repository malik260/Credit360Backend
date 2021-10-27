namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_SUB")]
    public partial class TBL_JOB_TYPE_SUB
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_JOB_TYPE_SUB()
        {
            TBL_JOB_REQUEST_DETAIL = new HashSet<TBL_JOB_REQUEST_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short JOB_SUB_TYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string JOB_SUB_TYPE_NAME { get; set; }

        public short JOBTYPEID { get; set; }

        public bool? REQUIRECHARGE { get; set; }
        public int? CHARGEFEEID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_DETAIL> TBL_JOB_REQUEST_DETAIL { get; set; }

        public virtual TBL_JOB_TYPE TBL_JOB_TYPE { get; set; }
    }
}
