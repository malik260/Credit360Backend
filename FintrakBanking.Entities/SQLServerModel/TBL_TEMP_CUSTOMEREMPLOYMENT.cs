namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_CUSTOMEREMPLOYMENT")]
    public partial class TBL_TEMP_CUSTOMEREMPLOYMENT
    {
        [Key]
        public int TEMPPLACEOFWORKID { get; set; }

        public int? PLACEOFWORKID { get; set; }

        public int CUSTOMERID { get; set; }

        public int? EMPLOYERID { get; set; }

        [Required]
        //[StringLength(200)]
        public string EMPLOYERNAME { get; set; }

        [Required]
        //[StringLength(200)]
        public string EMPLOYERADDRESS { get; set; }

        public int EMPLOYERSTATEID { get; set; }

        public int EMPLOYERCOUNTRYID { get; set; }

        [Required]
        //[StringLength(200)]
        public string OFFICEPHONE { get; set; }

        [Column(TypeName = "date")]
        public DateTime EMPLOYDATE { get; set; }

        //[StringLength(200)]
        public string PREVIOUSEMPLOYER { get; set; }

        public bool ACTIVE { get; set; }

        public bool ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }
    }
}
