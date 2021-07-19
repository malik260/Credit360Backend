namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_LOAN_DOCUMENT_TYPE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LOANDOCUMENTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string DOCUMENTTYPE { get; set; }
    }
}
