
namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class STG_CASA_DAILY_BALANCE
    {
        [Key]
        public int CASADAILYBALANCEID { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNAME { get; set; }

        public double ACCOUNTBALANCE { get; set; }

        public double TOTALINFLOW { get; set; }

        public double TOTALOUTFLOW { get; set; }

        public DateTime? BALANCEDATE { get; set; }

        //[StringLength(20)]
        public string CURRENCY { get; set; }

        public string SCHEME_CODE { get; set; }
    }
}
