namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_NEXTOFKIN")]
    public partial class TBL_CUSTOMER_NEXTOFKIN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NEXTOFKINID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        [StringLength(200)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(200)]
        public string LASTNAME { get; set; }

        [StringLength(200)]
        public string PHONENUMBER { get; set; }

        [StringLength(10)]
        public string GENDER { get; set; }

        [StringLength(50)]
        public string RELATIONSHIP { get; set; }

        [StringLength(200)]
        public string EMAIL { get; set; }

        [Required]
        [StringLength(200)]
        public string ADDRESS { get; set; }

        [StringLength(100)]
        public string NEAREST_LANDMARK { get; set; }

        public int? CITYID { get; set; }

        public int? ACTIVE { get; set; }

        public DateTime? DATEOFBIRTH { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
