namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOMER_EMPLOYER_TYPE_SUB")]
    public partial class TBL_CUSTOMER_EMPLOYER_TYPE_SUB
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CUSTOMER_EMPLOYER_TYPE_SUB()
        {
            TBL_CUSTOMER_EMPLOYER = new HashSet<TBL_CUSTOMER_EMPLOYER>();
            TBL_TEMP_CUSTOMER_EMPLOYER = new HashSet<TBL_TEMP_CUSTOMER_EMPLOYER>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short EMPLOYER_SUB_TYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string EMPLOYER_SUB_TYPE_NAME { get; set; }

        public short EMPLOYER_TYPEID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_EMPLOYER> TBL_CUSTOMER_EMPLOYER { get; set; }

        public virtual TBL_CUSTOMER_EMPLOYER_TYPE TBL_CUSTOMER_EMPLOYER_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CUSTOMER_EMPLOYER> TBL_TEMP_CUSTOMER_EMPLOYER { get; set; }
    }
}
