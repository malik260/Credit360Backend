namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_GRP_FS_CAPTN_DET")]
    public partial class TBL_CUSTOMER_GRP_FS_CAPTN_DET
    {
        [Key]
        public int GROUPFSDETAILID { get; set; }

        public int CUSTOMERGROUPID { get; set; }

        public int FSCAPTIONID { get; set; }

        [Column(TypeName = "date")]
        public DateTime FSDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal AMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION { get; set; }

        public virtual TBL_CUSTOMER_GROUP TBL_CUSTOMER_GROUP { get; set; }
    }
}
