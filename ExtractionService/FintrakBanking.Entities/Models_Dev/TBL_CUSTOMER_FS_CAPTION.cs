namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_FS_CAPTION")]
    public partial class TBL_CUSTOMER_FS_CAPTION
    {
        public TBL_CUSTOMER_FS_CAPTION()
        {
            TBL_CUSTOMER_FS_CAPTION_DETAIL = new HashSet<TBL_CUSTOMER_FS_CAPTION_DETAIL>();
            TBL_CUSTOMER_FS_RATIO_DETAIL = new HashSet<TBL_CUSTOMER_FS_RATIO_DETAIL>();
            TBL_CUSTOMER_GRP_FS_CAPTN_DET = new HashSet<TBL_CUSTOMER_GRP_FS_CAPTN_DET>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FSCAPTIONID { get; set; }

        [Required]
        [StringLength(200)]
        public string FSCAPTIONNAME { get; set; }

        public short FSCAPTIONGROUPID { get; set; }

        public bool ISRATIO { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CUSTOMER_FS_CAPTION_DETAIL> TBL_CUSTOMER_FS_CAPTION_DETAIL { get; set; }

        public virtual TBL_CUSTOMER_FS_CAPTION_GROUP TBL_CUSTOMER_FS_CAPTION_GROUP { get; set; }

        public virtual ICollection<TBL_CUSTOMER_FS_RATIO_DETAIL> TBL_CUSTOMER_FS_RATIO_DETAIL { get; set; }

        public virtual ICollection<TBL_CUSTOMER_GRP_FS_CAPTN_DET> TBL_CUSTOMER_GRP_FS_CAPTN_DET { get; set; }
    }
}
