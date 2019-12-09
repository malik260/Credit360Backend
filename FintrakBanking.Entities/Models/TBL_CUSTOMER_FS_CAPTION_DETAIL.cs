namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_FS_CAPTION_DETAIL")]
    public partial class TBL_CUSTOMER_FS_CAPTION_DETAIL
    {
        [Key]
        public int FSDETAILID { get; set; }

        public int CUSTOMERID { get; set; }

        public int FSCAPTIONID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime FSDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMOUNT { get; set; }

        public string TEXTVALUE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION { get; set; }
    }
}
