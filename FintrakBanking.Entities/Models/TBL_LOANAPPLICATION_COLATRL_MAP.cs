namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOANAPPLICATION_COLATRL_MAP")]
    public partial class TBL_LOANAPPLICATION_COLATRL_MAP
    {
        [Key]
        public int COLLATERAL_MAPPINGID { get; set; }

        public int? LOANAPPLICATIONID { get; set; }

        public int? COLLATERALCUSTOMERID { get; set; }

        [StringLength(50)]
        public string CAMREFERENCENUMBER { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual TBL_LOANAPPLICATION_COLATRL_MAP TBL_LOANAPPLICATION_COLATRL_MAP1 { get; set; }

        public virtual TBL_LOANAPPLICATION_COLATRL_MAP TBL_LOANAPPLICATION_COLATRL_MAP2 { get; set; }
    }
}
