namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_APPLICATION_DETL_STA")]
    public partial class TBL_LOAN_APPLICATION_DETL_STA
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STATUSID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string STATUSNAME { get; set; }
    }
}
