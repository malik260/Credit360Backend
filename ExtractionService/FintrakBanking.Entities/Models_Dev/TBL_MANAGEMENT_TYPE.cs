namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_MANAGEMENT_TYPE")]
    public partial class TBL_MANAGEMENT_TYPE
    {
        public TBL_MANAGEMENT_TYPE()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MANAGEMENTTYPEID { get; set; }

        [Required]
        [StringLength(250)]
        public string NAME { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }
    }
}
