namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CITY_CLASS")]
    public partial class TBL_CITY_CLASS
    {
        public TBL_CITY_CLASS()
        {
            TBL_CITY = new HashSet<TBL_CITY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CITYCLASSID { get; set; }

        [Required]
        [StringLength(50)]
        public string CITYCLASSNAME { get; set; }

        public virtual ICollection<TBL_CITY> TBL_CITY { get; set; }
    }
}
