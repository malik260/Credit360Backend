namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("SYSDIAGRAMS")]
    public partial class SYSDIAGRAM
    {
        [Required]
        //[StringLength(128)]
        public string NAME { get; set; }

        public int PRINCIPAL_ID { get; set; }

        [Key]
        public int DIAGRAM_ID { get; set; }

        public int? VERSION { get; set; }

        public byte[] DEFINITION { get; set; }
    }
}
