namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_BULK_DISBURS_SCH_FEES")]
    public partial class TBL_BULK_DISBURS_SCH_FEES
    {
        [Key]
        public int SCHEMEFEEID { get; set; }

        public int DISBURSESCHEMEID { get; set; }

        public int CHARGEFEEID { get; set; }

        public bool HASCONCESSION { get; set; }
        public int? APPROVALSTATUSID { get; set; }
        //public short COMPANYID { get; set; }
        //public int CREATEDBY { get; set; }

        //public int LASTUPDATEDBY { get; set; }

        //public int DATETIMECREATED { get; set; }
        //public int DATETIMEUPDATED { get; set; }
        //public int DELETED { get; set; }
        //public int DELETEDBY { get; set; }
        //public int DATETIMEDELETED { get; set; }
    }
}
