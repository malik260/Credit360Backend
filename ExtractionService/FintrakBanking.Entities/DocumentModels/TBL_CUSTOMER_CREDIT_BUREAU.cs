namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_CUSTOMER_CREDIT_BUREAU
    {
        [Key]
        public int DOCUMENTID { get; set; }

        public int CUSTOMERCREDITBUREAUID { get; set; }

        [Required]
        ////[StringLength(250)]
        public string DOCUMENT_TITLE { get; set; }

        [Required]
        ////[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        ////[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
