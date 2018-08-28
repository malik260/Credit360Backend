namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_CUSTOMER_CREDIT_BUREAU")]
    public partial class TBL_CUSTOMER_CREDIT_BUREAU
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_CREDIT_BUREAU()
        {
            TBL_LOAN_APPLTN_CREDIT_BUREAU = new HashSet<TBL_LOAN_APPLTN_CREDIT_BUREAU>();
        }

        [Key]
        public int CUSTOMERCREDITBUREAUID { get; set; }

        public int CUSTOMERID { get; set; }

        public short CREDITBUREAUID { get; set; }

        public int? COMPANYDIRECTORID { get; set; }

        [Column(TypeName = "money")]
        public decimal CHARGEAMOUNT { get; set; }

        public bool ISREPORTOKAY { get; set; }

        public bool USEDINTEGRATION { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DATECOMPLETED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

        public virtual TBL_CUSTOMER_COMPANY_DIRECTOR TBL_CUSTOMER_COMPANY_DIRECTOR { get; set; }

        public virtual TBL_CREDIT_BUREAU TBL_CREDIT_BUREAU { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLTN_CREDIT_BUREAU> TBL_LOAN_APPLTN_CREDIT_BUREAU { get; set; }
    }
}
