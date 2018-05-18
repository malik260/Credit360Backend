namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_DEAL_CLASSIFICATION")]
    public partial class TBL_DEAL_CLASSIFICATION
    {
        public TBL_DEAL_CLASSIFICATION()
        {
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DEALCLASSIFICATIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string CLASSIFICATION { get; set; }

        [Required]
        [StringLength(10)]
        public string CODE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
