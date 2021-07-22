namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_FIELD_OPTION")]
    public partial class TBL_CUSTOM_FIELD_OPTION
    {
        [Key]
        public int CUSTOMFIELDOPTIONSID { get; set; }

        public int CUSTOMFIELDID { get; set; }

        //[StringLength(50)]
        public string OPTIONSKEY { get; set; }

        //[StringLength(50)]
        public string OPTIONSVALUE { get; set; }
    }
}
