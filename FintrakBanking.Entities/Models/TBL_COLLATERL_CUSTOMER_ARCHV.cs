using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERL_CUSTOMER_ARCHV")]
    public partial class TBL_COLLATERL_CUSTOMER_ARCHV
    {
        [Key]
        public int COLLATERALCUSTOMERARCHID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public short COLLATERALSUBTYPEID { get; set; }

        //[Required]
        ////[StringLength(50)]
        public string COLLATERALCODE { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        public int COMPANYID { get; set; }

        public bool ALLOWSHARING { get; set; }

        public bool? ISLOCATIONBASED { get; set; }

        public int? VALUATIONCYCLE { get; set; }

        public decimal COLLATERALVALUE { get; set; }

        public double HAIRCUT { get; set; }

        public int? CUSTOMERID { get; set; }

        public int? CUSTOMERGROUPID { get; set; }

        ////[StringLength(50)]
        public string CAMREFNUMBER { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }
        public int? COLLATERALRELEASESTATUSID { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int APPROVALSTATUS { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }
        public int? LOANAPPLICATIONID { get; set; }
        public int? COLLATERALUSAGESTATUSID { get; set; }

        public string RELATEDCOLLATERALCODE { get; set; }

        public DateTime? VALIDTILL { get; set; }
        public string COLLATERALSUMMARY { get; set; }
    }
}
