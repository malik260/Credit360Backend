namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LC_USSANCE")]
    public partial class TBL_LC_USSANCE
    {
        [Key]
        public int LCUSSANCEID { get; set; }

        public int LCISSUANCEID { get; set; }

        public int? USSANCETENOR { get; set; }

        public decimal USSANCEAMOUNT { get; set; }
        public decimal USSANCEAMOUNTLOCAL { get; set; }

        public double? USSANCERATE { get; set; }

        public int? USANCEAMOUNTCURRENCYID { get; set; }
        public int? USANCEAPPLICATIONSTATUSID { get; set; }
        public int? USANCEAPPROVALSTATUSID { get; set; }

        public virtual TBL_LC_ISSUANCE TBL_LC_ISSUANCE { get; set; }

        public DateTime? LCUSSANCEEFFECTIVEDATE { get; set; }

        public DateTime? LCUSSANCEMATURITYDATE { get; set; }

        public string USANCEREF { get; set; }
        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

    }
}
