namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_CUSTOMER_NEXTOFKIN")]
    public partial class TBL_TEMP_CUSTOMER_NEXTOFKIN
    {
        [Key]
        public int TEMPNEXTOFKINID { get; set; }

        public int? NEXTOFKINID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        [StringLength(200)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(200)]
        public string LASTNAME { get; set; }

        [StringLength(200)]
        public string PHONENUMBER { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? DATEOFBIRTH { get; set; }

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

        public bool? ACTIVE { get; set; }

        public bool ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }
    }
}
