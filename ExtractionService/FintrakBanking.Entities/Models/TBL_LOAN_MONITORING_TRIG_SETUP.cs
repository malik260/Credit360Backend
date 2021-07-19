namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_MONITORING_TRIG_SETUP")]
    public partial class TBL_LOAN_MONITORING_TRIG_SETUP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_MONITORING_TRIG_SETUP()
        {
            TBL_LOAN_APPLICATN_DETL_MTRIG = new HashSet<TBL_LOAN_APPLICATN_DETL_MTRIG>();
            TBL_LOAN_MONITORING_TRIGGER = new HashSet<TBL_LOAN_MONITORING_TRIGGER>();
        }

        [Key]
        public int MONITORING_TRIGGERID { get; set; }

        public int? OPERATIONID { get; set; }

        [Required]
        //[StringLength(800)]
        public string MONITORING_TRIGGER_NAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATN_DETL_MTRIG> TBL_LOAN_APPLICATN_DETL_MTRIG { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_MONITORING_TRIGGER> TBL_LOAN_MONITORING_TRIGGER { get; set; }
    }
}
