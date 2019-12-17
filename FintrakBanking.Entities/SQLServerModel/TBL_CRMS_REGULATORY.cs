namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CRMS_REGULATORY")]
    public partial class TBL_CRMS_REGULATORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CRMS_REGULATORY()
        {
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            TBL_PRODUCT_BEHAVIOUR = new HashSet<TBL_PRODUCT_BEHAVIOUR>();
        }

        [Key]
        public int CRMSREGULATORYID { get; set; }

        public int COMPANYID { get; set; }

        public short CRMSTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CODE { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_BEHAVIOUR> TBL_PRODUCT_BEHAVIOUR { get; set; }

        public virtual TBL_CRMS_TYPE TBL_CRMS_TYPE { get; set; }
    }
}
