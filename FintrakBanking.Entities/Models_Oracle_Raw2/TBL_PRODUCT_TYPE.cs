namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_TYPE")]
    public partial class TBL_PRODUCT_TYPE
    {
        public TBL_PRODUCT_TYPE()
        {
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            TBL_LOAN_FORCE_DEBIT = new HashSet<TBL_LOAN_FORCE_DEBIT>();
            TBL_LOAN_PAST_DUE = new HashSet<TBL_LOAN_PAST_DUE>();
            TBL_LOAN_RECOVERY_PLAN = new HashSet<TBL_LOAN_RECOVERY_PLAN>();
            TBL_LOAN_SCHEDULE_TYPE_PRODUCT = new HashSet<TBL_LOAN_SCHEDULE_TYPE_PRODUCT>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_STAFF_ACCOUNT_HISTORY = new HashSet<TBL_STAFF_ACCOUNT_HISTORY>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PRODUCTTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTTYPENAME { get; set; }

        public int PRODUCTGROUPID { get; set; }

        public int REQUIREPRINCIPALGL { get; set; }

        public int REQUIREPRINCIPALGL2 { get; set; }

        public int REQUIREINTERESTINCOMEEXPENSEGL { get; set; }

        public int REQUIRE_INT_RECEIVABL_PAYABLGL { get; set; }

        public int REQUIREDORMANTGL { get; set; }

        public int REQUIREPREMIUMDISCOUNTGL { get; set; }

        public int REQUIREOVERDRAWNGL { get; set; }

        public int DEALCLASSIFICATIONID { get; set; }

        public int REQUIRERATE { get; set; }

        public int REQUIRETENOR { get; set; }

        public int REQUIRESCHEDULETYPE { get; set; }

        public int DELETED { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }

        public virtual TBL_DEAL_CLASSIFICATION TBL_DEAL_CLASSIFICATION { get; set; }

        public virtual ICollection<TBL_LOAN_FORCE_DEBIT> TBL_LOAN_FORCE_DEBIT { get; set; }

        public virtual ICollection<TBL_LOAN_PAST_DUE> TBL_LOAN_PAST_DUE { get; set; }

        public virtual ICollection<TBL_LOAN_RECOVERY_PLAN> TBL_LOAN_RECOVERY_PLAN { get; set; }

        public virtual ICollection<TBL_LOAN_SCHEDULE_TYPE_PRODUCT> TBL_LOAN_SCHEDULE_TYPE_PRODUCT { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT_GROUP TBL_PRODUCT_GROUP { get; set; }

        public virtual ICollection<TBL_STAFF_ACCOUNT_HISTORY> TBL_STAFF_ACCOUNT_HISTORY { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
