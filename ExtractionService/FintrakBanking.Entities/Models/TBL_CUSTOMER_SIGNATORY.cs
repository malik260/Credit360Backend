namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_SIGNATORY")]
    public partial class TBL_CUSTOMER_SIGNATORY
    {
        [Key]
        public int CUSTOMER_SIGNATORYID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        //[StringLength(50)]
        public string SURNAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string FIRSTNAME { get; set; }

        //[StringLength(50)]
        public string MIDDLENAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string BVN { get; set; }

        [Required]
        //[StringLength(10)]
        public string GENDER { get; set; }
    }
}
