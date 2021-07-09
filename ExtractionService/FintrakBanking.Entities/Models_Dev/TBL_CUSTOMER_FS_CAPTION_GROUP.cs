namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_FS_CAPTION_GROUP")]
    public partial class TBL_CUSTOMER_FS_CAPTION_GROUP
    {
        public TBL_CUSTOMER_FS_CAPTION_GROUP()
        {
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_CUSTOMER_FS_CAPTION = new HashSet<TBL_CUSTOMER_FS_CAPTION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FSCAPTIONGROUPID { get; set; }

        [Required]
        [StringLength(50)]
        public string FSCAPTIONGROUPNAME { get; set; }

        public int POSITION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_CUSTOMER_FS_CAPTION> TBL_CUSTOMER_FS_CAPTION { get; set; }
    }
}
