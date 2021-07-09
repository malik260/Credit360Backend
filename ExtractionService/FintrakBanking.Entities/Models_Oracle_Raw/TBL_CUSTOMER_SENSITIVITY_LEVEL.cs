namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_SENSITIVITY_LEVEL")]
    public partial class TBL_CUSTOMER_SENSITIVITY_LEVEL
    {
        public TBL_CUSTOMER_SENSITIVITY_LEVEL()
        {
            TBL_CUSTOMER = new HashSet<TBL_CUSTOMER>();
            TBL_STAFF = new HashSet<TBL_STAFF>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERSENSITIVITYLEVELID { get; set; }

        [StringLength(200)]
        public string DESCRIPTION { get; set; }

        public virtual ICollection<TBL_CUSTOMER> TBL_CUSTOMER { get; set; }

        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }
    }
}
