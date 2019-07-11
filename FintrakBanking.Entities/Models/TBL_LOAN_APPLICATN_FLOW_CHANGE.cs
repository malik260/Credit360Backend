
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATN_FLOW_CHANGE")]
    public partial class TBL_LOAN_APPLICATN_FLOW_CHANGE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]

        [Key]
        public short FLOWCHANGEID { get; set; }

        [Required]
        [StringLength(20)]
        public string SOURCEPLACEHOLDER { get; set; }

        [Required]
        [StringLength(20)]
        public string TARGETPLACEHOLDER { get; set; }

        [Required]
        [StringLength(20)]
        public string SOURCEURL { get; set; }

        public int ROUTEOPERATIONID { get; set; }
        
        public bool DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime DATETIMECREATED { get; set; }

    }
}
