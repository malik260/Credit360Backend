namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_PRODUCT_CLASS")]
    public partial class TBL_PRODUCT_CLASS
    {
        public TBL_PRODUCT_CLASS()
        {
            TBL_APPROVAL_GROUP_MAPPING = new HashSet<TBL_APPROVAL_GROUP_MAPPING>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCTCLASSID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCLASSNAME { get; set; }

        public short PRODUCTCLASSTYPEID { get; set; }

        public short PRODUCT_CLASS_PROCESSID { get; set; }

        public short CUSTOMERTYPEID { get; set; }

        public virtual ICollection<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }

        public virtual TBL_CUSTOMER_TYPE TBL_CUSTOMER_TYPE { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }

        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        public virtual TBL_PRODUCT_CLASS_PROCESS TBL_PRODUCT_CLASS_PROCESS { get; set; }

        public virtual TBL_PRODUCT_CLASS_TYPE TBL_PRODUCT_CLASS_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
