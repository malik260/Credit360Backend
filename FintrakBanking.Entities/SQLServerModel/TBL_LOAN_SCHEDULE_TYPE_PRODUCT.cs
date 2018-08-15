namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_SCHEDULE_TYPE_PRODUCT")]
    public partial class TBL_LOAN_SCHEDULE_TYPE_PRODUCT
    {
        [Key]
        public int SCHEDULEPRODUCTTYPEID { get; set; }

        public short SCHEDULETYPEID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_LOAN_SCHEDULE_TYPE TBL_LOAN_SCHEDULE_TYPE { get; set; }
    }
}
