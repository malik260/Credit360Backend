
namespace FintrakBanking.Entities.Models
{
    using FintrakBanking.Entities.DocumentModels;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PRODUCT_DOC_MAPPING")]
    public partial class TBL_PRODUCT_DOCUMENT_MAPPING
    {
        [Key]
        public int PRODUCTDOCMAPID { get; set; }

        public int PRODUCTID { get; set; }

        public int DOCUMENTID { get; set; }

        public bool REQUIRED { get; set; }

        public   DateTime DATETIMEUPDATED { get; set; }

        public int DOCUMENTTYPEID { get; set; }

        public DateTime DATETIMEDELETED { get; set; }
        public bool DELETED { get; set; }
        public virtual TBL_DOCUMENT_TYPE TBL_DOCUMENT_TYPE { get; set; }


    }
}










