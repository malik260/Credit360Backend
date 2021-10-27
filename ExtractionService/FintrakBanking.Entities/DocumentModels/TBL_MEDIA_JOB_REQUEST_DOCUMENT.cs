namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TBL_MEDIA_JOB_REQUEST_DOCUMENT
    {
        [Key]
        public int DOCUMENTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string JOBREQUESTCODE { get; set; }

        [Required]
        //[StringLength(250)]
        public string DOCUMENTTITLE { get; set; }

        public short DOCUMENTTYPEID { get; set; }

        [Required]
        //[StringLength(400)]
        public string FILENAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        //[StringLength(50)]
        public string PHYSICALFILENUMBER { get; set; }

        //[StringLength(250)]
        public string PHYSICALLOCATION { get; set; }

        public int CREATEDBY { get; set; }
    }
}
