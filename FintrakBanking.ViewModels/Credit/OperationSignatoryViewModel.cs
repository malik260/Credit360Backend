using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OperationSignatoryViewModel : GeneralEntity
    {
        public int operationSignatoryId { get; set; }
        public int operationId { get; set; }
        public int? targetId { get; set; }
        public int signatoryId { get; set; }
    }

    //public class AuthorisedSignatoryViewModel : GeneralEntity
    //{
    //    public int signatoryId { get; set; }
    //    public string signatoryName { get; set; }
    //    public string signatoryTitle { get; set; }
    //    public string signatoryInitials { get; set; }
    //}
}
