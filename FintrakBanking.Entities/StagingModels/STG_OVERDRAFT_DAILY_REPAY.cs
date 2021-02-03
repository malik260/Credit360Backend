namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_OVERDRAFT_DAILY_REPAY")]
    public partial class STG_OVERDRAFT_DAILY_REPAY
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long OVERDARFTDAILYREPAYID { get; set; }

        //[StringLength(255)]
        public string CUSTOMERACCOUNTNUMBER { get; set; }

        //[StringLength(255)]
        public short LOANSYSTEMTYPEID { get; set; }
        
        public decimal ACCOUNTBALANCE { get; set; }

        public decimal DEBITTURNOVER { get; set; }

        public decimal CREDITTURNOVER { get; set; }
        public DateTime TRANSACTIONDATE { get; set; }
        public  bool STATUS  { get; set; }
    }
}

