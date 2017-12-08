namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LIMIT_DETAIL")]
    public partial class TBL_LIMIT_DETAIL
    {
        [Key]
        public int LIMITDETAILID { get; set; }

        public int LIMITTYPEID { get; set; }

        public int LIMITID { get; set; }

        public int TARGETID { get; set; }

        [Column(TypeName = "money")]
        public decimal MINIMUMVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal MAXIMUMVALUE { get; set; }

        public short LIMITFREQUENCYTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_FREQUENCY_TYPE TBL_FREQUENCY_TYPE { get; set; }

        public virtual TBL_LIMIT TBL_LIMIT { get; set; }

        public virtual TBL_LIMIT_TYPE TBL_LIMIT_TYPE { get; set; }
    }
}
