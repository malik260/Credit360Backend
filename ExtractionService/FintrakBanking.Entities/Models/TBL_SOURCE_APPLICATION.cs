namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_SOURCE_APPLICATION")]
    public partial class TBL_SOURCE_APPLICATION
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short APPLICATIONID { get; set; }

        //[StringLength(150)]
        public string APPLICATIONNAME { get; set; }
    }
}
