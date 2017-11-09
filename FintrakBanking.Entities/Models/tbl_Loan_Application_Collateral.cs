namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_COLLATERAL")]
    public partial class TBL_LOAN_APPLICATION_COLLATERAL
    {
        [Key]
        public int LOANAPPCOLLATERALID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
