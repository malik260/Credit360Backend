namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_COMPANY_BENEFICIA")]
    public partial class TBL_CUSTOMER_COMPANY_BENEFICIA
    {
        [Key]
        public int COMPANY_BENEFICIARYID { get; set; }

        public int COMPANYDIRECTORID { get; set; }

        [Required]
        [StringLength(50)]
        public string SURNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [StringLength(200)]
        public string MIDDLENAME { get; set; }

        [Required]
        [StringLength(20)]
        public string CUSTOMERBVN { get; set; }

        [StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public int NUMBEROFSHARES { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public int? UPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_CUSTOMER_COMPANY_DIRECTOR TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }
    }
}
