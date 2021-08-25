namespace FintrakBanking.Entities.DocumentModels
{
    using FintrakBanking.Entities.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOCUMENT_UPLOAD")]
    public partial class TBL_DOCUMENT_UPLOAD
    {
        [Key]
        public int DOCUMENTUPLOADID { get; set; }

        public int DOCUMENTTYPEID { get; set; }

        [Required]
        ////[StringLength(250)]
        public string FILENAME { get; set; }

        [Required]
        ////[StringLength(10)]
        public string FILEEXTENSION { get; set; }

        public int FILESIZE { get; set; }

        ////[StringLength(3)]
        public string FILESIZEUNIT { get; set; }

        [Required]
        public byte[] FILEDATA { get; set; }

        public int? COMPANYID { get; set; }

        public DateTime? ISSUEDATE { get; set; }

        public DateTime? EXPIRYDATE { get; set; }

        ////[StringLength(20)]
        public string PHYSICALFILENUMBER { get; set; }

        ////[StringLength(20)]
        public string PHYSICALLOCATION { get; set; }
        public bool ISORIGINALCOPY { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? SOURCE { get; set; }
        public int? EDMSDOCID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_DOCUMENT_TYPE TBL_DOCUMENT_TYPE { get; set; }

    }
}