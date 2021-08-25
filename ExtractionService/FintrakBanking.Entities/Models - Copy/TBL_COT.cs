namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COT")]
    public partial class TBL_COT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COTID { get; set; }

        public int? ACCOUNTID { get; set; }

        public decimal? COTACCOUNTAMOUNT { get; set; }

        [StringLength(50)]
        public string COTCREATEDBY { get; set; }

        public int? ISCURRENT { get; set; }

        public DateTime? COTDATE { get; set; }
    }
}
