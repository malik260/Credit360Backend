namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STATE")]
    public partial class TBL_STATE
    {
        public TBL_STATE()
        {
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
            TBL_LOCALGOVERNMENT = new HashSet<TBL_LOCALGOVERNMENT>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STATEID { get; set; }

        public short COUNTRYID { get; set; }

        [Required]
        [StringLength(100)]
        public string STATENAME { get; set; }

        [StringLength(10)]
        public string STATECODE { get; set; }

        public int? REGIONID { get; set; }

        public decimal COLLATERALSEARCHCHARGEAMOUNT { get; set; }

        public decimal? CHARTINGAMOUNT { get; set; }

        public decimal? VERIFICATIONAMOUNT { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        public virtual ICollection<TBL_LOCALGOVERNMENT> TBL_LOCALGOVERNMENT { get; set; }

        public virtual TBL_REGION TBL_REGION { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }
    }
}
