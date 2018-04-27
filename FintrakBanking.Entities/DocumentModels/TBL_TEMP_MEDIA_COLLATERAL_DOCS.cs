namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_TEMP_MEDIA_COLLATERAL_DOCS
    {
        [Key]
        public int DOCUMENTID { get; set; }

        [Required]
        [StringLength(100)]
        public string DOCUMENTCODE { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        public int? TARGETID { get; set; }

        public bool ISPRIMARYDOCUMENT { get; set; }

        [Required]
        [StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        [StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public int CREATEDBY { get; set; }
    }
}
