namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_SETUP_COMPANY")]
    public partial class TBL_SETUP_COMPANY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GLOBALSETUPID { get; set; }

        public int COMPANYID { get; set; }

        public double UNAUTHORISD_OVERDRAFT_INT_RATE { get; set; }

        public double PASTDUEINDEFAULT_INTERESTRATE { get; set; }

        public int LEGAL_CHARGE_GLACCOUNTID { get; set; }

        [StringLength(200)]
        public string APIURL { get; set; }

        [StringLength(100)]
        public string APIKEY { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }
    }
}
