using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_VALUATION_PRE")]
    public partial class TBL_COLLATERAL_VALUATION_PRE
    {
        [Key]
        public int VALUATIONPREREQUISITEID { get; set; }

        //public int COLLATERALCUSTOMERID { get; set; }

        public int VALUATIONREQUESTTYPEID { get; set; }

        public string VALUATIONCOMMENT { get; set; }
        public string REFERENCENUMBER { get; set; }

        public int? COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
        public int? OPERATIONID { get; set; }

        public int? APPROVALSTATUSID { get; set; }
        public int COLLATERALVALUATIONID { get; set; }
        public int? NUMBEROFTIMESAPPROVE { get; set; }

    }
}
