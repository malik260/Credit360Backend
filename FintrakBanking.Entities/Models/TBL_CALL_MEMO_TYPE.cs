namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CALL_MEMO_TYPE")]
    public partial class TBL_CALL_MEMO_TYPE
    {
       
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CALLLIMITTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string NAME { get; set; }

       
    }
}
