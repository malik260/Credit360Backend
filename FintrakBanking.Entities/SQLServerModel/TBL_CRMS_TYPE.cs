namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CRMS_TYPE")]
    public partial class TBL_CRMS_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CRMS_TYPE()
        {
            TBL_CRMS_REGULATORY = new HashSet<TBL_CRMS_REGULATORY>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CRMSTYPEID { get; set; }

        [Required]
        //[StringLength(150)]
        public string DESCRIPTION { get; set; }

        public int COMPANYID { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CRMS_REGULATORY> TBL_CRMS_REGULATORY { get; set; }
    }
}
