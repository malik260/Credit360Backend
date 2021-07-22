namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TEMP_COMPANY_BENEFICIA")]
    public partial class TBL_TEMP_COMPANY_BENEFICIA
    {
        [Key]
        public int TEMPCOMPANY_BENEFICIARYID { get; set; }

        public int? TEMPCOMPANYDIRECTORID { get; set; }

        public int? COMPANYDIRECTORID { get; set; }

        [Required]
        //[StringLength(50)]
        public string SURNAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string FIRSTNAME { get; set; }

        //[StringLength(200)]
        public string MIDDLENAME { get; set; }

        [Required]
        //[StringLength(20)]
        public string CUSTOMERBVN { get; set; }

        //[StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public int NUMBEROFSHARES { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        //[StringLength(500)]
        public string ADDRESS { get; set; }

        //[StringLength(50)]
        public string PHONENUMBER { get; set; }

        //[StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public virtual TBL_TEMP_CUSTOMER_DIRECTOR TBL_TEMP_CUSTOMER_DIRECTOR { get; set; }
    }
}
