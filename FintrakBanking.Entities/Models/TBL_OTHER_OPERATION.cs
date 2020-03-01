namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_OTHER_OPERATION")]
    public partial class TBL_OTHER_OPERATION
    {

        [Key]
        public int OTHEROPERATIONID { get; set; }

        [Required]
        //[StringLength(100)]
        public string OPERATIONNAME { get; set; }

    }
}
