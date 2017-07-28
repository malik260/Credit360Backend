using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Notification
{
   public  class NotificationViewModel
    {
        public int massageCount { get; set; }
        public string message { get; set; }
        public string operationURL { get; set; }
    }
}
