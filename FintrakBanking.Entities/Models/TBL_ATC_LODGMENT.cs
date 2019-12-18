namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ATC_LODGMENT")]
    public partial class TBL_ATC_LODGMENT
    {
        public TBL_ATC_LODGMENT()
        {
            TBL_ATC_LODGMENT_DETAIL = new HashSet<TBL_ATC_LODGMENT_DETAIL>();
        }

        [Key]
        public int ATCLODGMENTID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
     //   //[StringLength(20)]
        public int ATCTYPEID { get; set; }

        [Required]
        //[StringLength(200)]
        public string DESCRIPTION { get; set; }

        [Required]
        //[StringLength(20)]
        public string DEPOT { get; set; }

        public decimal UNITVALUE { get; set; }

        public int UNITNUMBER { get; set; }

        
        public string CERTIFICATENUMBER { get; set; }

        public int STATUSID { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int? BRANCHID { get; set; }
        public int NUMBEROFBAGS { get; set; }
        public DateTime? DATETIMEAPPROVED { get; set; }
        public int? CURRENCYID { get; set; }

        public DateTime? DATETIMERELEASED { get; set; }


        public virtual ICollection<TBL_ATC_LODGMENT_DETAIL> TBL_ATC_LODGMENT_DETAIL { get; set; }

    }
}
        /*

        public virtual DbSet<TBL_ATC_LODGMENT> TBL_ATC_LODGMENT { get; set; }

            modelBuilder.Entity<TBL_ATC_LODGMENT>()
                .HasMany(e => e.TBL_ATC_LODGMENT_DETAIL)
                .WithRequired(e => e.TBL_ATC_LODGMENT)
                .WillCascadeOnDelete(false);

        */
