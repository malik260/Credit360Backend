namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_REGION")]
    public partial class TBL_REGION
    {
        public TBL_REGION()
        {
            TBL_STATE = new HashSet<TBL_STATE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int REGIONID { get; set; }

        public short COUNTRYID { get; set; }

        [Required]
        [StringLength(100)]
        public string REGIONNAME { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        public virtual ICollection<TBL_STATE> TBL_STATE { get; set; }
    }
}
