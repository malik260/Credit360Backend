namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CUSTOMER_CLIENT_SUPPLIER")]
    public partial class TBL_CUSTOMER_CLIENT_SUPPLIER
    {
        [Key]
        public int CLIENT_SUPPLIERID { get; set; }

        public int CUSTOMERID { get; set; }

        public short CUSTOMERTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string FIRSTNAME { get; set; }

        [StringLength(100)]
        public string MIDDLENAME { get; set; }

        [StringLength(100)]
        public string LASTNAME { get; set; }

        [StringLength(500)]
        public string ADDRESS { get; set; }

        [StringLength(50)]
        public string PHONENUMBER { get; set; }

        [StringLength(50)]
        public string EMAILADDRESS { get; set; }

        public short CLIENT_SUPPLIERTYPEID { get; set; }

        public int CREATEDBY { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATECREATED { get; set; }

        public int? UPDATEDBY { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE TBL_CUSTOMER_CLIENT_SUPPLIER_TYPE { get; set; }
    }
}
