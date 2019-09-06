
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_BULK_DISBURSE_SCHEME")]
    public partial class TBL_LOAN_BULK_DISBURSE_SCHEME
    {
        [Key]
        public int DISBURSESCHEMEID { get; set; }
        public int PRODUCTID { get; set; }
        public short? SCHEDULEMETHODID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int CURRENCYID { get; set; }
        public string SCHEMECODE { get; set; }
        public int TENOR { get; set; }

        public string SCHEMENAME { get; set; }
        public float INTERESTRATE { get; set; }
        public int? PRODUCTPRICEINDEXID { get; set; }
        public short? APPROVALSTATUSID { get; set; }

        public short COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
        public DateTime DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime DATETIMEDELETED { get; set; }
        public int SCHEDULEDAYCOUNTCONVENTIONID { get; set; }
        public int INTERESTFREQUENCYTYPEID { get; set; }
        public int PRINCIPALFREQUENCYTYPEID { get; set; }

    }
}









