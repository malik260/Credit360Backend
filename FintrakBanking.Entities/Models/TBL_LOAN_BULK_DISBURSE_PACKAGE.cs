namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_BULK_DISBURSE_PACKAGE")]
    public partial class TBL_LOAN_BULK_DISBURSE_PACKAGE
    {
        [Key]
        public int DISBURSEMENTPACKAGEID { get; set; }

        public int GROUPCUSTOMERID { get; set; }

        public DateTime STARTDATE { get; set; }

        public DateTime ENDDATE { get; set; }

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








