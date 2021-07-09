namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CITY")]
    public partial class TBL_CITY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CITY()
        {
            TBL_BRANCH = new HashSet<TBL_BRANCH>();
            TBL_COLLATERAL_IMMOVE_PROPERTY = new HashSet<TBL_COLLATERAL_IMMOVE_PROPERTY>();
            TBL_CUSTOMER_EMPLOYER = new HashSet<TBL_CUSTOMER_EMPLOYER>();
            TBL_CUSTOMER_NEXTOFKIN = new HashSet<TBL_CUSTOMER_NEXTOFKIN>();
            TBL_LOAN_MARKET = new HashSet<TBL_LOAN_MARKET>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
            TBL_TEMP_CUSTOMER_EMPLOYER = new HashSet<TBL_TEMP_CUSTOMER_EMPLOYER>();
            TBL_TEMP_STAFF = new HashSet<TBL_TEMP_STAFF>();
            TBL_TEMP_COLLATERAL_IMMOV_PROP = new HashSet<TBL_TEMP_COLLATERAL_IMMOV_PROP>();
        }

        [Key]
        public int CITYID { get; set; }

        [Required]
        //[StringLength(150)]
        public string CITYNAME { get; set; }

        //[StringLength(50)]
        public string CRMSCODE { get; set; }
        public int LOCALGOVERNMENTID { get; set; }

        public short? CITYCLASSID { get; set; }

        public bool ALLOWEDFORCOLLATERAL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual TBL_CITY_CLASS TBL_CITY_CLASS { get; set; }

        public virtual TBL_LOCALGOVERNMENT TBL_LOCALGOVERNMENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> TBL_COLLATERAL_IMMOVE_PROPERTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_EMPLOYER> TBL_CUSTOMER_EMPLOYER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_NEXTOFKIN> TBL_CUSTOMER_NEXTOFKIN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_MARKET> TBL_LOAN_MARKET { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_EMPLOYER> TBL_TEMP_CUSTOMER_EMPLOYER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOV_PROP> TBL_TEMP_COLLATERAL_IMMOV_PROP { get; set; }
    }
}
