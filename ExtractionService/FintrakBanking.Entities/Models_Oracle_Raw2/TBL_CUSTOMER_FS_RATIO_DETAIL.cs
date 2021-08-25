namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_FS_RATIO_DETAIL")]
    public partial class TBL_CUSTOMER_FS_RATIO_DETAIL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RATIODETAILID { get; set; }

        public int RATIOCAPTIONID { get; set; }

        public int FSCAPTIONID { get; set; }

        public int DIVISORTYPEID { get; set; }

        public decimal MULTIPLIER { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public int VALUETYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION TBL_CUSTOMER_FS_CAPTION1 { get; set; }

        public virtual TBL_CUSTOMER_FS_RATIO_DIVI_TYP TBL_CUSTOMER_FS_RATIO_DIVI_TYP { get; set; }

        public virtual TBL_CUSTOMER_FS_RATIO_VALUETYP TBL_CUSTOMER_FS_RATIO_VALUETYP { get; set; }
    }
}
