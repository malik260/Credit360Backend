using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum JobOtherSubTypeEnum
    {
        LegalSearch = LegalJob.Search,
        LegalVerification = LegalJob.Verification,
        LegalCharting = LegalJob.Charting,
        OtherLegalJobs = LegalJob.Others,
        MiddleOfficeVerification = 4,
        CAMSOLCheck = 5,
        BlackBookCheck = 6,
        OtherBlackBookCheckFunction = 7
    }

    public enum JobSubTypeEnum
    {
        CollateralRelated = 1,
        BondAndGuaranteeVetting = 2,
    }

    public enum LegalJob
    {
        Search = 1,
        Verification = 2,
        Charting = 3,
        Others = 8
    }
}

