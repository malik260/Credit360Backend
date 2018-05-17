namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOCALGOVERNMENT")]
    public partial class TBL_LOCALGOVERNMENT
    {
        public TBL_LOCALGOVERNMENT()
        {
            TBL_CITY = new HashSet<TBL_CITY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LOCALGOVERNMENTID { get; set; }

        [Required]
        [StringLength(200)]
        public string NAME { get; set; }

        public int STATEID { get; set; }

        public virtual ICollection<TBL_CITY> TBL_CITY { get; set; }

        public virtual TBL_STATE TBL_STATE { get; set; }
    }
}
