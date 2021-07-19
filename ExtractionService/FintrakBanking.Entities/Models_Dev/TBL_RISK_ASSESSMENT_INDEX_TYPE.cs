namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_RISK_ASSESSMENT_INDEX_TYPE")]
    public partial class TBL_RISK_ASSESSMENT_INDEX_TYPE
    {
        public TBL_RISK_ASSESSMENT_INDEX_TYPE()
        {
            TBL_RISK_ASSESSMENT_INDEX = new HashSet<TBL_RISK_ASSESSMENT_INDEX>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int INDEXTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string INDEXTYPENAME { get; set; }

        public virtual ICollection<TBL_RISK_ASSESSMENT_INDEX> TBL_RISK_ASSESSMENT_INDEX { get; set; }
    }
}
