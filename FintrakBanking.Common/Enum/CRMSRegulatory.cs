using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
 public  enum CRMSRegulatory
    {
        Government = 147,
        Parastatals_MDA = 148
    }


    public enum CRMSTemplate
    {
        Template100 = 1,
        Template200 = 2,
        Template300 = 3,
        Template300Fee = 4,
        Template300Directors = 5,
        Template600 = 6,
        Template400A = 7,
        Template400B = 8,
        Template400C = 9,
    }

    public enum CRMSTypeEnum
    {
        LoanType = 1,
        FeeType = 2,
        RepaymentAgreementType = 3,
        SecuredCollateralType = 4,
        UnsecuredCollateralType = 5,
        RepaymentSourceType = 6,
        LegalStatusType = 7,
        RelationshipType = 9,
        CompanySize = 10,
        FundingSource =11
    }
}
