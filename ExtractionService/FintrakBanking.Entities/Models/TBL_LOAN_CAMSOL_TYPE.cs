using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    public class TBL_LOAN_CAMSOL_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_CAMSOL_TYPE()
        {
            TBL_LOAN_CAMSOL = new HashSet<TBL_LOAN_CAMSOL>();
        }

        [Key]
        public int CAMSOLTYPEID { get; set; }
        public string CAMSOLTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }

    }
}
