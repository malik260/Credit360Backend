namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_EMPLOYMENTHISTORY")]
    public partial class TBL_CUSTOMER_EMPLOYMENTHISTORY
    {
        [Key]
        public int PLACEOFWORKID { get; set; }

        public int CUSTOMERID { get; set; }

        public int? EMPLOYERID { get; set; }

        [Required]
        [StringLength(200)]
        public string EMPLOYERNAME { get; set; }

        [Required]
        [StringLength(200)]
        public string EMPLOYERADDRESS { get; set; }

        public int EMPLOYERSTATEID { get; set; }

        public int EMPLOYERCOUNTRYID { get; set; }

        [Required]
        [StringLength(200)]
        public string OFFICEPHONE { get; set; }

        [Column(TypeName = "date")]
        public DateTime EMPLOYDATE { get; set; }

        [StringLength(200)]
        public string PREVIOUSEMPLOYER { get; set; }

        public bool ACTIVE { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_EMPLOYER TBL_CUSTOMER_EMPLOYER { get; set; }
    }
}
