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

        public string PACKAGENAME { get; set; }

        public DateTime STARTDATE { get; set; }

        public DateTime ENDDATE { get; set; }

        public short COMPANYID { get; set; }


        public int CREATEDBY { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
        public DateTime DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime DATETIMEDELETED { get; set; }
        public string PACKAGEDESCRIPTION { get; set; }


    }
}








