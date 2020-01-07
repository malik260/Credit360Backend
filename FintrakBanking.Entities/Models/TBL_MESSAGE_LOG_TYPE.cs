namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_MESSAGE_LOG_TYPE")]
    public partial class TBL_MESSAGE_LOG_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_MESSAGE_LOG_TYPE()
        {
            //TBL_MESSAGE_LOG = new HashSet<TBL_MESSAGE_LOG>();
            //TBL_MONITORING_ALERT_SETUP = new HashSet<TBL_MONITORING_ALERT_SETUP>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short MESSAGETYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string MESSAGETYPENAME { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_MESSAGE_LOG> TBL_MESSAGE_LOG { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<TBL_MONITORING_ALERT_SETUP> TBL_MONITORING_ALERT_SETUP { get; set; }
    }
}
