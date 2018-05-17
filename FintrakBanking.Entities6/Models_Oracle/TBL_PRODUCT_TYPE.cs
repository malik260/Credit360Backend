namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PRODUCT_TYPE")]
    public partial class TBL_PRODUCT_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_TYPE()
        {
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
            TBL_LOAN_FORCE_DEBIT = new HashSet<TBL_LOAN_FORCE_DEBIT>();
            TBL_LOAN_PAST_DUE = new HashSet<TBL_LOAN_PAST_DUE>();
            TBL_LOAN_RECOVERY_PLAN = new HashSet<TBL_LOAN_RECOVERY_PLAN>();
            TBL_LOAN_SCHEDULE_TYPE_PRODUCT = new HashSet<TBL_LOAN_SCHEDULE_TYPE_PRODUCT>();
            TBL_STAFF_ACCOUNT_HISTORY = new HashSet<TBL_STAFF_ACCOUNT_HISTORY>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
            
        }

        [Key]
        public short PRODUCTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTTYPENAME { get; set; }

        public short PRODUCTGROUPID { get; set; }

        public bool REQUIREPRINCIPALGL { get; set; }

        public bool REQUIREPRINCIPALGL2 { get; set; }

        public bool REQUIREINTERESTINCOMEEXPENSEGL { get; set; }

        public bool REQUIRE_INT_RECEIVABL_PAYABLGL { get; set; }

        public bool REQUIREDORMANTGL { get; set; }

        public bool REQUIREPREMIUMDISCOUNTGL { get; set; }

        public bool REQUIREOVERDRAWNGL { get; set; }

        public short DEALCLASSIFICATIONID { get; set; }

        public bool REQUIRERATE { get; set; }

        public bool REQUIRETENOR { get; set; }

        public bool REQUIRESCHEDULETYPE { get; set; }

        public bool DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }       

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT_GROUP TBL_PRODUCT_GROUP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FORCE_DEBIT> TBL_LOAN_FORCE_DEBIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PAST_DUE> TBL_LOAN_PAST_DUE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_RECOVERY_PLAN> TBL_LOAN_RECOVERY_PLAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_SCHEDULE_TYPE_PRODUCT> TBL_LOAN_SCHEDULE_TYPE_PRODUCT { get; set; }

        public virtual TBL_DEAL_CLASSIFICATION TBL_DEAL_CLASSIFICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
       
    }
}
