namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_COLLATERL")]
    public partial class TBL_LOAN_APPLICATION_COLLATERL
    {
        [Key]
        public int LOANAPPCOLLATERALID { get; set; }
        [ForeignKey("TBL_COLLATERAL_CUSTOMER")]
        public int COLLATERALCUSTOMERID { get; set; }
        [ForeignKey("TBL_LOAN_APPLICATION")]
        public int LOANAPPLICATIONID { get; set; }

        public bool? LEGAL_FEE_TAKEN { get; set; }

        //[Column(TypeName = "money")]
        public decimal? LEGAL_FEE_AMOUNT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? LEGAL_FEE_DATE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? LOANAPPLICATIONDETAILID { get; set; }

        public int? CUSTOMERID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public decimal COLLATERALCOVERAGE { get; set; }
        public decimal BALANCEAVAILABLE { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }
    }
}
