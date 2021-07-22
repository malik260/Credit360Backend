namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_FEE_INTERVAL")]
    public partial class TBL_FEE_INTERVAL
    {
        public TBL_FEE_INTERVAL()
        {
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FEEINTERVALID { get; set; }

        [StringLength(50)]
        public string FEEINTERVALNAME { get; set; }

        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }

        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
    }
}
