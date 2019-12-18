namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_FS_RATIO_DETAIL")]
    public partial class TBL_CUSTOMER_FS_RATIO_DETAIL
    {
        [Key]
        public int RATIODETAILID { get; set; }

        public int RATIOCAPTIONID { get; set; }

        public int FSCAPTIONID { get; set; }

        public short DIVISORTYPEID { get; set; }

        public double MULTIPLIER { get; set; }

        //[StringLength(50)]
        public string DESCRIPTION { get; set; }

        public short VALUETYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION1 { get; set; }

        public virtual TBL_CUSTOMER_FS_RATIO_DIVI_TYP TBL_CUSTOMER_FS_RATIO_DIVI_TYP { get; set; }

        public virtual TBL_CUSTOMER_FS_RATIO_VALUETYP TBL_CUSTOMER_FS_RATIO_VALUETYP { get; set; }
    }
}
