
namespace FintrakBanking.Entities.Models
{
    using FintrakBanking.Entities.DocumentModels;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOC_MAPPING")]
    public partial class TBL_DOC_MAPPING
    {
        [Key]
        public int PRODUCTDOCMAPID { get; set; }

        public int? PRODUCTID { get; set; }
        public int? PRODUCTCLASSID { get; set; }
        public int? OPERATIONID { get; set; }

        public bool MAPTOPRODUCTCLASS { get; set; }
        public bool MAPTOPRODUCT { get; set; }
        public bool MAPTOOPERATION { get; set; }
        public bool ISREQUIRED { get; set; }

        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }

        public DateTime DATETIMEUPDATED { get; set; }

        public int DOCUMENTTYPEID { get; set; }

        public DateTime DATETIMEDELETED { get; set; }
        public bool DELETED { get; set; }
        public virtual TBL_DOCUMENT_TYPE TBL_DOCUMENT_TYPE { get; set; }


    }
}










