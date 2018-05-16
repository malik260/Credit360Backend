namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CUSTOMER_BVN")]
    public partial class TBL_CUSTOMER_BVN
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CUSTOMERBVNID { get; set; }

        public int CUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string SURNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string FIRSTNAME { get; set; }

        [Required]
        [StringLength(20)]
        public string BANKVERIFICATIONNUMBER { get; set; }

        public bool ISVALIDBVN { get; set; }

        public bool ISPOLITICALLYEXPOSED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }
    }
}
