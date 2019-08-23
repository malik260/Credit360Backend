
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

        public int DISBURSEMENTPACKAGEID { get; set; }
        public short? SCHEDULEMETHODID { get; set; }

        public int TENOR { get; set; }

        public int INTERESTRATE { get; set; }
        public int? PRODUCTPRICEINDEXID { get; set; }
        public bool INCLUDEPRODUCTFEES { get; set; }

        public short? APPROVALSTATUSID { get; set; }

        public short COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public int DATETIMECREATED { get; set; }
        public int DATETIMEUPDATED { get; set; }
        public int DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public int DATETIMEDELETED { get; set; }

    }
}









