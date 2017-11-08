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

        public int? CASAACCOUNTID { get; set; }

        [StringLength(50)]
        public string COLLATERALREFERENCENUMBER { get; set; }

        [Column(TypeName = "money")]
        public decimal? COLLATERALVALUE { get; set; }

        public bool? ISBANKACCOUNT { get; set; }

        public int? CITYID { get; set; }

        [StringLength(50)]
        public string DOCUMENTTITLE { get; set; }

        public double? LATITUDE { get; set; }

        public double? LONGITUDE { get; set; }

        [StringLength(50)]
        public string LOCATIONADDRESS { get; set; }

        [StringLength(250)]
        public string NEARESTBUSSTOP { get; set; }

        [StringLength(250)]
        public string NEARESTLANDMARK { get; set; }

        [StringLength(500)]
        public string OTHERINFORMATIONS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }
    }
}
