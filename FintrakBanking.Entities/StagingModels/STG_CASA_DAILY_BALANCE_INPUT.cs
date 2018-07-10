namespace DAL.CWG
{
    using System;
    using System.ComponentModel.DataAnnotations; 
    public partial class STG_CASA_DAILY_BALANCE_INPUT
    {
        [Required]
        public DateTime BALANCEDATE { get; set; }

        [Required]
        [StringLength(20)]
        public string ACCOUNTNUMBER { get; set; }
    }
}
