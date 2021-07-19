namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CURRENCY_RATECODE")]
    public partial class TBL_CURRENCY_RATECODE
    {
        public TBL_CURRENCY_RATECODE()
        {
            TBL_CURRENCY_EXCHANGERATE = new HashSet<TBL_CURRENCY_EXCHANGERATE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RATECODEID { get; set; }

        [Required]
        [StringLength(50)]
        public string RATECODE { get; set; }

        [Required]
        [StringLength(100)]
        public string RATECODEDESCRIPTION { get; set; }

        public virtual ICollection<TBL_CURRENCY_EXCHANGERATE> TBL_CURRENCY_EXCHANGERATE { get; set; }
    }
}
