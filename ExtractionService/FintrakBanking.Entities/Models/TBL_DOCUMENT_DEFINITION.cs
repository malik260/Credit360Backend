
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_DOCUMENT_DEFINITION")]
    public partial class TBL_DOCUMENT_DEFINITION
    {
        [Key]
        public int DOCUMENTDEFINITIONID { get; set; }

        public string DOCUMENTTITLE { get; set; }

        public bool INUSE { get; set; }
    }
}
