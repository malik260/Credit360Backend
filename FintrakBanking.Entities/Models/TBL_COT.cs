namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_COT
    {
        [Key]
        public int COTID { get; set; }

        public int? ACCOUNTID { get; set; }

        //[Column(TypeName = "money")]
        public decimal? COTACCOUNTAMOUNT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? COTDATE { get; set; }

        //[StringLength(50)]
        public string COTCREATEDBY { get; set; }

        public bool? ISCURRENT { get; set; }
    }
}
