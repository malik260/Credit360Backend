namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_COMPANY_DIREC_TYP")]
    public partial class TBL_CUSTOMER_COMPANY_DIREC_TYP
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_COMPANY_DIREC_TYP()
        {
            TBL_CUSTOMER_COMPANY_DIRECTOR = new HashSet<TBL_CUSTOMER_COMPANY_DIRECTOR>();
        }

        [Key]
        public short COMPANYDIRECTORYTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string COMPANYDIRECTORYTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_COMPANY_DIRECTOR> TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }
    }
}
