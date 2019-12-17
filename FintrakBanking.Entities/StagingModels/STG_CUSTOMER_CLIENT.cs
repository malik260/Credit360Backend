namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("STG_CUSTOMER_CLIENT")]
    public partial class STG_CUSTOMER_CLIENT
    {
        [Key]
        [Column(Order = 0)]
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

        [Key]
        [Column(Order = 1)]
        //[StringLength(255)]
        public string CUSTOMERCODE { get; set; }

        //[StringLength(255)]
        public string CLIENTTYPE { get; set; }
    }
}
