namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_ACCREDITEDCONSULTANT_TYPE")]
    public partial class TBL_ACCREDITEDCONSULTANT_TYPE
    {
        public TBL_ACCREDITEDCONSULTANT_TYPE()
        {
            TBL_ACCREDITEDCONSULTANT = new HashSet<TBL_ACCREDITEDCONSULTANT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCREDITEDCONSULTANTID { get; set; }

        [Required]
        [StringLength(100)]
        public string NAME { get; set; }

        public virtual ICollection<TBL_ACCREDITEDCONSULTANT> TBL_ACCREDITEDCONSULTANT { get; set; }
    }
}
