namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CITY")]
    public partial class TBL_CITY
    {
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
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CITYID { get; set; }

        [Required]
        [StringLength(150)]
        public string CITYNAME { get; set; }

        public int LOCALGOVERNMENTID { get; set; }

        public int CITYCLASSID { get; set; }

        public int ALLOWEDFORCOLLATERAL { get; set; }

        public virtual ICollection<TBL_BRANCH> TBL_BRANCH { get; set; }

        public virtual TBL_CITY_CLASS TBL_CITY_CLASS { get; set; }

        public virtual TBL_LOCALGOVERNMENT TBL_LOCALGOVERNMENT { get; set; }

        public virtual ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> TBL_COLLATERAL_IMMOVE_PROPERTY { get; set; }

        public virtual ICollection<TBL_CUSTOMER_EMPLOYER> TBL_CUSTOMER_EMPLOYER { get; set; }

        public virtual ICollection<TBL_CUSTOMER_NEXTOFKIN> TBL_CUSTOMER_NEXTOFKIN { get; set; }

        public virtual ICollection<TBL_LOAN_MARKET> TBL_LOAN_MARKET { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }

        public virtual ICollection<TBL_TEMP_CUSTOMER_EMPLOYER> TBL_TEMP_CUSTOMER_EMPLOYER { get; set; }

        public virtual ICollection<TBL_TEMP_STAFF> TBL_TEMP_STAFF { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOV_PROP> TBL_TEMP_COLLATERAL_IMMOV_PROP { get; set; }
    }
}
