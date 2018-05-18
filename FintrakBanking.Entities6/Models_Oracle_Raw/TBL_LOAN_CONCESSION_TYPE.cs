namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_CONCESSION_TYPE")]
    public partial class TBL_LOAN_CONCESSION_TYPE
    {
        public TBL_LOAN_CONCESSION_TYPE()
        {
            TBL_LOAN_RATE_FEE_CONCESSION = new HashSet<TBL_LOAN_RATE_FEE_CONCESSION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONCESSIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string CONCESSIONTYPENAME { get; set; }

        public virtual ICollection<TBL_LOAN_RATE_FEE_CONCESSION> TBL_LOAN_RATE_FEE_CONCESSION { get; set; }
    }
}
