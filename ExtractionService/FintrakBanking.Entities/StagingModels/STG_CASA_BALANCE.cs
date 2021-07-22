namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_CASA_BALANCE")]
    public partial class STG_CASA_BALANCE
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        //[StringLength(255)]
        public string ACCOUNTNUMBER { get; set; }

        //[StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        //[StringLength(255)]
        public string ACCOUNTNAME { get; set; }

        public decimal? ACCOUNTBALANCE { get; set; }

        public decimal? TOTALINFLOW { get; set; }

        public decimal? TOTALOUTFLOW { get; set; }

        public DateTime? BALANCEDATE { get; set; }
    }
}
