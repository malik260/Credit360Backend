namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_NEXTOFKIN")]
    public partial class TBL_CUSTOMER_NEXTOFKIN
    {
        [Key]
        public int NEXTOFKINID { get; set; }

        [Required]
        [StringLength(200)]
        public string NEXTOFKINNAME { get; set; }

        [Required]
        [StringLength(200)]
        public string NEXTOFKINFIRSTNAME { get; set; }

        [StringLength(200)]
        public string NEXTOFKINPHONENUMBER { get; set; }

        [StringLength(200)]
        public string NESTOFKINEMAIL { get; set; }

        [Required]
        [StringLength(200)]
        public string NESTOFKINADDRESS { get; set; }

        public int CUSTOMERID { get; set; }

        public bool? ACTIVE { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
