namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks; 

    [Table("TBL_LOAN_REPRICING_MODE")]
    public partial class TBL_LOAN_REPRICING_MODE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_REPRICING_MODE()
        {
            
        }

        [Key]
        public int REPRICINGMODEID { get; set; }

        //[StringLength(200)]
        public string REPRICINGMODENAME { get; set; }
    }
}


