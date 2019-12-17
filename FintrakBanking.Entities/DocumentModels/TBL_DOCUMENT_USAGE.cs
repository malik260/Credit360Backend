namespace FintrakBanking.Entities.DocumentModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOCUMENT_USAGE")]
    public partial class TBL_DOCUMENT_USAGE
    {
        [Key]
        public int DOCUMENTUSAGEID { get; set; }

        public int DOCUMENTUPLOADID { get; set; }

        public int TARGETID { get; set; }

        ////[StringLength(20)]
        public string TARGETCODE { get; set; }

        ////[StringLength(20)]
        public string TARGETREFERENCENUMBER { get; set; }

        ////[StringLength(20)]
        public string DOCUMENTCODE { get; set; }

        ////[StringLength(250)]
        public string DOCUMENTTITLE { get; set; }

        ////[StringLength(20)]
        public string CUSTOMERCODE { get; set; }

        public int OPERATIONID { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        public int? DOCUMENTSTATUSID { get; set; }

        public bool ISPRIMARYDOCUMENT { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

    }
}