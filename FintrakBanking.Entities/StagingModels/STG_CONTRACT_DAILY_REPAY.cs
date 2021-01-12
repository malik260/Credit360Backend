namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_CONTRACT_DAILY_REPAY")]
    public partial class STG_CONTRACT_DAILY_REPAY
    {
        [Key]
        public long CONTRACTDAILYREPAYID { get; set; }

        //[StringLength(255)]
        public string CONTRACTREFERENCENUMBER { get; set; }

        //[StringLength(255)]
        public string CUSTOMERACCOUNTNUMBER { get; set; }

        //[StringLength(255)]
        public short LOANSYSTEMTYPEID { get; set; }

        public string BRANCHCODE { get; set; }

        public string PAYMENTDESCRIPTION { get; set; }

        public DateTime DUEDATE { get; set; }

        public DateTime PAYMENTDATE { get; set; }
        public decimal AMOUNTPAID { get; set; }
        public bool STATUS { get; set; }
    }
}

