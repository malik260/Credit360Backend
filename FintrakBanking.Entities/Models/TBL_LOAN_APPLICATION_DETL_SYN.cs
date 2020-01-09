namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_DETL_SYN")]
    public partial class TBL_LOAN_APPLICATION_DETL_SYN
    {
    
        [Key]
        public int SYNDICATIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        
        [Required]
        //[StringLength(50)]
        public string BANKCODE { get; set; }

        [Required]
        //[StringLength(50)]
        public string BANKNAME { get; set; }

   
        public decimal AMOUNTCONTRIBUTED { get; set; }


        public int PARTY_TYPEID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
