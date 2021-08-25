namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_SIGNATORY")]
    public partial class TBL_CUSTOMER_SIGNATORY
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        //[StringLength(255)]
        public string FIRSTNAME { get; set; }

        //[StringLength(255)]
        public string LASTNAME { get; set; }

        //[StringLength(255)]
        public string ADDRESS { get; set; }

        //[StringLength(255)]
        public string PHONENUMBER { get; set; }

        //[StringLength(255)]
        public string EMAIL { get; set; }

        //[StringLength(255)]
        public string BVN { get; set; }

        //[StringLength(255)]
        public string CUSTOMERCODE { get; set; }
    }
}
