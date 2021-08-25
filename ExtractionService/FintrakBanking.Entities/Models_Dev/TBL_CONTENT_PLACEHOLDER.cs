namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CONTENT_PLACEHOLDER")]
    public partial class TBL_CONTENT_PLACEHOLDER
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CONTENTPLACEHOLDERID { get; set; }

        [Required]
        [StringLength(50)]
        public string CONTENTPLACEHOLDER { get; set; }

        [StringLength(50)]
        public string COLLUMNNAME { get; set; }
    }
}
