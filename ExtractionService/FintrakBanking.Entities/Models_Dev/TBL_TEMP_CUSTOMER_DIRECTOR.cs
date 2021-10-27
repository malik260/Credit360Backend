namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_CUSTOMER_DIRECTOR")]
    public partial class TBL_TEMP_CUSTOMER_DIRECTOR
    {
        public TBL_TEMP_CUSTOMER_DIRECTOR()
        {
            TBL_TEMP_COMPANY_BENEFICIA = new HashSet<TBL_TEMP_COMPANY_BENEFICIA>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOMPANYDIRECTORID { get; set; }

        public int? COMPANYDIRECTORID { get; set; }

        public int CUSTOMERID { get; set; }

        [StringLength(50)]
        public string SURNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [StringLength(50)]
        public string MIDDLENAME { get; set; }

        public short CUSTOMERTYPEID { get; set; }

        public short COMPANYDIRECTORTYPEID { get; set; }

        [StringLength(20)]
        public string CUSTOMERBVN { get; set; }

        [StringLength(50)]
        public string CUSTOMERNIN { get; set; }

        public double SHAREHOLDINGPERCENTAGE { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        [StringLength(500)]
        public string OTHERS { get; set; }

        [StringLength(50)]
        public string REGISTRATION_NUMBER { get; set; }

        [StringLength(50)]
        public string TAX_NUMBER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATECREATED { get; set; }

        public bool ISCURRENT { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public virtual ICollection<TBL_TEMP_COMPANY_BENEFICIA> TBL_TEMP_COMPANY_BENEFICIA { get; set; }
    }
}
