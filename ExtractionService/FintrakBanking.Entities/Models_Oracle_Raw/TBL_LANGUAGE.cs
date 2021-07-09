namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LANGUAGE")]
    public partial class TBL_LANGUAGE
    {
        public TBL_LANGUAGE()
        {
            TBL_COMPANY = new HashSet<TBL_COMPANY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LANGUAGEID { get; set; }

        [Required]
        [StringLength(30)]
        public string LANGUAGECODE { get; set; }

        [Required]
        [StringLength(50)]
        public string LANGUAGENAME { get; set; }

        public virtual ICollection<TBL_COMPANY> TBL_COMPANY { get; set; }
    }
}
