namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_SOLICITOR")]
    public partial class TBL_SOLICITOR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_SOLICITOR()
        {
            TBL_SOLICITOR_STATE_MAPPING = new HashSet<TBL_SOLICITOR_STATE_MAPPING>();
        }

        [Key]
        public int SOLICITORID { get; set; }

        public int COMPANYID { get; set; }

        [Required]
        [StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        [Required]
        [StringLength(150)]
        public string SOLICITORNAME { get; set; }

        [Required]
        [StringLength(500)]
        public string ADDRESS { get; set; }

        [Required]
        [StringLength(250)]
        public string CONTACTPERSON { get; set; }

        [Required]
        [StringLength(250)]
        public string EMAIL { get; set; }

        [Required]
        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_SOLICITOR_STATE_MAPPING> TBL_SOLICITOR_STATE_MAPPING { get; set; }
    }
}
