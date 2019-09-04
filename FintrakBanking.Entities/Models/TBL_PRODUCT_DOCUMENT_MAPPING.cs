
namespace FintrakBanking.Entities.Models
{
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

        public int DOCUMENTDEFINITIONID { get; set; }

        public bool ISREQUIRED { get; set; }

        public bool INUSE { get; set; }
        

    }
}










