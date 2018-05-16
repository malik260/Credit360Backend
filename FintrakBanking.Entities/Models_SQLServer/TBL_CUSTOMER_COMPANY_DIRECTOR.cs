namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_COMPANY_DIRECTOR")]
    public partial class TBL_CUSTOMER_COMPANY_DIRECTOR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_COMPANY_DIRECTOR()
        {
            TBL_CUSTOMER_COMPANY_BENEFICIA = new HashSet<TBL_CUSTOMER_COMPANY_BENEFICIA>();
            TBL_CUSTOMER_CREDIT_BUREAU = new HashSet<TBL_CUSTOMER_CREDIT_BUREAU>();
        }

        [Key]
        public int COMPANYDIRECTORID { get; set; }

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

        public int? UPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_COMPANY_BENEFICIA> TBL_CUSTOMER_COMPANY_BENEFICIA { get; set; }

        public virtual TBL_CUSTOMER_COMPANY_DIREC_TYP TBL_CUSTOMER_COMPANY_DIREC_TYP { get; set; }

        public virtual TBL_CUSTOMER_TYPE TBL_CUSTOMER_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
    }
}
