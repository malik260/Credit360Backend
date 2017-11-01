namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LIMIT")]
    public partial class TBL_LIMIT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LIMIT()
        {
            TBL_LIMIT_DETAIL = new HashSet<TBL_LIMIT_DETAIL>();
        }

        [Key]
        public int LIMITID { get; set; }

        [Required]
        [StringLength(200)]
        public string LIMITNAME { get; set; }

        public int COMPANYID { get; set; }

        public int LIMITVALUETYPEID { get; set; }

        public int LIMITMETRICID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LIMIT_DETAIL> TBL_LIMIT_DETAIL { get; set; }

        public virtual TBL_LIMIT_METRIC TBL_LIMIT_METRIC { get; set; }

        public virtual TBL_LIMIT_VALUE_TYPE TBL_LIMIT_VALUE_TYPE { get; set; }
    }
}
