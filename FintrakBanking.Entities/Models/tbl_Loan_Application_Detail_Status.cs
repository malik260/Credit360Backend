namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_DETAIL_STATUS")]
    public partial class TBL_LOAN_APPLICATION_DETAIL_STATUS
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short STATUSID { get; set; }

        [Required]
        [StringLength(50)]
        public string STATUSNAME { get; set; }
    }
}
