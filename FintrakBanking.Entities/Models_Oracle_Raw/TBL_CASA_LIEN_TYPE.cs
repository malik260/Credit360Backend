namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CASA_LIEN_TYPE")]
    public partial class TBL_CASA_LIEN_TYPE
    {
        public TBL_CASA_LIEN_TYPE()
        {
            TBL_CASA_LIEN = new HashSet<TBL_CASA_LIEN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LIENTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string LIENTYPENAME { get; set; }

        public virtual ICollection<TBL_CASA_LIEN> TBL_CASA_LIEN { get; set; }
    }
}
