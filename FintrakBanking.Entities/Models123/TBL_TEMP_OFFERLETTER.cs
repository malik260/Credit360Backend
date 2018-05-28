namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_OFFERLETTER")]
    public partial class TBL_TEMP_OFFERLETTER
    {
        [Key]
        public int DOCUMENTID { get; set; }

        public string LOANAPPLICATIONDOCUMENT { get; set; }

        [StringLength(50)]
        public string APPLICATIONREFERENCENUMBER { get; set; }

        public short? PRODUCTID { get; set; }

        [StringLength(500)]
        public string COMMENTS { get; set; }

        public bool? ISACCEPTED { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
