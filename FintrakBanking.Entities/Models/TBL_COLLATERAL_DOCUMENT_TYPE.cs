namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_DOCUMENT_TYPE")]
    public partial class TBL_COLLATERAL_DOCUMENT_TYPE
    {
       
        [Key]
        public short DOCUMENTTYPEID { get; set; }

        //[Required]
        ////[StringLength(250)]
        public string DOCUMENTTYPENAME { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

    
    }
}
