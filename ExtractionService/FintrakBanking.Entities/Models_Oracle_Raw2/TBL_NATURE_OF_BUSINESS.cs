namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_NATURE_OF_BUSINESS")]
    public partial class TBL_NATURE_OF_BUSINESS
    {
        public TBL_NATURE_OF_BUSINESS()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NATUREOFBUSINESSID { get; set; }

        [Required]
        [StringLength(250)]
        public string NAME { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }
    }
}
