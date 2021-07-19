namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_TYPE_SUB_CLASS")]
    public partial class TBL_JOB_TYPE_SUB_CLASS
    {
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        //public TBL_JOB_TYPE_SUB_CLASS()
        //{
        //    TBL_JOB_TYPE_SUB = new HashSet<TBL_JOB_TYPE_SUB>();
        //}

        [Key]
        public int JOB_SUB_TYPE_CLASSID { get; set; }

        [Required]
        //[StringLength(100)]
        public string JOB_SUB_TYPE_CLASS_NAME { get; set; }

        [Required]
        public short JOB_SUB_TYPEID { get; set; }

        public decimal? DEFAULTCHARGEAMOUNT { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_JOB_TYPE_SUB> TBL_JOB_TYPE_SUB { get; set; }

    }
}
