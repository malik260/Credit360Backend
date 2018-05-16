namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_PRUDENT_GUIDE_TYPE")]
    public partial class TBL_LOAN_PRUDENT_GUIDE_TYPE
    {
        public TBL_LOAN_PRUDENT_GUIDE_TYPE()
        {
            TBL_LOAN_PRUDENTIALGUIDELINE = new HashSet<TBL_LOAN_PRUDENTIALGUIDELINE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRUDENTIALGUIDELINETYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRUDENTIALGUIDELINETYPENAME { get; set; }

        public virtual ICollection<TBL_LOAN_PRUDENTIALGUIDELINE> TBL_LOAN_PRUDENTIALGUIDELINE { get; set; }
    }
}
