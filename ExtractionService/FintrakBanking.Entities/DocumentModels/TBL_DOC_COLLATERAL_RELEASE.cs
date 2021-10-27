namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_DOC_COLLATERAL_RELEASE
    { 
        [Key]
        public int DOCUMENTID { get; set; }

        public int COLLATERALRELEASEID { get; set; }

        public string COLLATERALCODE { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }
        public int? APPROVALSTATUSID { get; set; }

        [Required]
        //////[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        ////[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int CREATEDBY { get; set; }
    }
}
