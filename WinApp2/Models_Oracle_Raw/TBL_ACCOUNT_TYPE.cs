namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_ACCOUNT_TYPE")]
    public partial class TBL_ACCOUNT_TYPE
    {
        public TBL_ACCOUNT_TYPE()
        {
            TBL_CHART_OF_ACCOUNT = new HashSet<TBL_CHART_OF_ACCOUNT>();
            TBL_TEMP_CHART_OF_ACCOUNT = new HashSet<TBL_TEMP_CHART_OF_ACCOUNT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ACCOUNTTYPEID { get; set; }

        public int ACCOUNTTYPECODE { get; set; }

        [Required]
        [StringLength(100)]
        public string ACCOUNTTYPENAME { get; set; }

        public int ACCOUNTCATEGORYID { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_ACCOUNT_CATEGORY TBL_ACCOUNT_CATEGORY { get; set; }

        public virtual ICollection<TBL_CHART_OF_ACCOUNT> TBL_CHART_OF_ACCOUNT { get; set; }

        public virtual ICollection<TBL_TEMP_CHART_OF_ACCOUNT> TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
    }
}
