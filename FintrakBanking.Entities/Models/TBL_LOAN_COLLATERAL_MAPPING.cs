namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_COLLATERAL_MAPPING")]
    public partial class TBL_LOAN_COLLATERAL_MAPPING
    {
        [Key]
        public int LOANCOLLATERALMAPPINGID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public int LOANID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public bool ISRELEASED { get; set; }

        public short? RELEASEAPPROVALSTATUSID { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
