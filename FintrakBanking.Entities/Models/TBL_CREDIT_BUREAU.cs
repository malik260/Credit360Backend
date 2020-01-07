namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CREDIT_BUREAU")]
    public partial class TBL_CREDIT_BUREAU
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CREDIT_BUREAU()
        {
            TBL_CUSTOMER_CREDIT_BUREAU = new HashSet<TBL_CUSTOMER_CREDIT_BUREAU>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CREDITBUREAUID { get; set; }

        [Required]
        //[StringLength(300)]
        public string CREDITBUREAUNAME { get; set; }

        //[Column(TypeName = "money")]
        public decimal CORPORATE_CHARGEAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal INDIVIDUAL_CHARGEAMOUNT { get; set; }

        public bool INUSE { get; set; }

        public int GLACCOUNTID { get; set; }

        public bool ISMANDATORY { get; set; }

        public bool USEINTEGRATION { get; set; }

        //[StringLength(50)]
        public string USERNAME { get; set; }

        //[StringLength(50)]
        public string PASSWORD { get; set; }

        //[StringLength(1000)]
        public string TOKEN { get; set; }
        public int? CHECKLISTITEMID { get; set; }

        public virtual TBL_CHART_OF_ACCOUNT TBL_CHART_OF_ACCOUNT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_CREDIT_BUREAU> TBL_CUSTOMER_CREDIT_BUREAU { get; set; }
    }
}
