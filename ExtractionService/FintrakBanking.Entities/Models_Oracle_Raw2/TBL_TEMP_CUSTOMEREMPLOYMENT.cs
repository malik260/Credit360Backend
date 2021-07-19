namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMEREMPLOYMENT")]
    public partial class TBL_TEMP_CUSTOMEREMPLOYMENT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPPLACEOFWORKID { get; set; }

        public int? PLACEOFWORKID { get; set; }

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

        [StringLength(200)]
        public string PREVIOUSEMPLOYER { get; set; }

        public int ACTIVE { get; set; }

        public int ISCURRENT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public DateTime EMPLOYDATE { get; set; }
    }
}
