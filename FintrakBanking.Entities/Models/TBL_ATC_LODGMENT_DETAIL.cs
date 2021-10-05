namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ATC_LODGMENT_DETAIL")]
    public partial class TBL_ATC_LODGMENT_DETAIL
    {
        [Key]
        public int ATCLODGMENTDETAILID { get; set; }

        [Required]
        //[StringLength(100)]
        public string DETAIL { get; set; }

        public decimal VALUE { get; set; }

        public int ATCLODGMENTID { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_ATC_LODGMENT TBL_ATC_LODGMENT { get; set; }
        //public string DESCRIPTION { get; set; }
    }
}
        /*

        public virtual DbSet<TBL_ATC_LODGMENT_DETAIL> TBL_ATC_LODGMENT_DETAIL { get; set; }

            modelBuilder.Entity<TBL_ATC_LODGMENT>()
                .HasMany(e => e.TBL_ATC_LODGMENT_DETAIL)
                .WithRequired(e => e.TBL_ATC_LODGMENT)
                .WillCascadeOnDelete(false);

        */
