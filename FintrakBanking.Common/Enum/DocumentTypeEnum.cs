using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
   public enum DocumentTypeEnum
    {
        LoanApplicationApprovalOuputDocument =30,
        InsurancePolicy = 147
    }

    public enum LoanDocumentTypeEnum
    {
        Legal = 1,
        Collateral = 2,
        BankStatement = 3,
        Others = 4,
        BookingRequest = 5
    }
}
