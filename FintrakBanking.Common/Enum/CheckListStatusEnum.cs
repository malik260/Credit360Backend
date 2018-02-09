using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
  
    public enum CheckListStatusEnum 
    {
       Waived = 2,
       Provided = 3,
       Deferred = 4,
       Yes = 5,
       No = 6
    }

   public  enum ChecklistErrorEnum
    {
        NegetiveChecklist = 3,
        IncompleteChecklist = 2,
        GoodChecklist = 1
    }
}
