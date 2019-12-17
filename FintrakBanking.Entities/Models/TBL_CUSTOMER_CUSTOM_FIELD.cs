namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_CUSTOM_FIELD")]
    public partial class TBL_CUSTOMER_CUSTOM_FIELD
    {
        [Key]
        public int CUSTOMERCUSTOMFIELDID { get; set; }

        public int? CUSTOMERID { get; set; }

        public int DISPLAYORDER { get; set; }

        [Required]
        //[StringLength(50)]
        public string LABEL { get; set; }

        //[StringLength(250)]
        public string VALUE { get; set; }

        public bool? SHOWBYDEFAULT { get; set; }

        //[StringLength(100)]
        public string CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
