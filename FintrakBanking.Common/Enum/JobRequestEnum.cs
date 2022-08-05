using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum JobTypeEnum
    {
        legal = 1,
        middleOfficeVerification  = 2,
        camsolCheck = 4,
        blackBookCheck   = 5,
        others = 6

    }

    public enum JobSubTypeEnum
    {
        CollateralRelated = 1,
        BondsAndGauranteeVetting = 2,
        Others = 3,
        MiddleOfficeVerification = 4,
        CAMSOLCheck = 5,
        BlackBookCheck = 6,
        ConfirmationOfTreasuryBills = 7,
        ConfirmationOfDealSlip = 8,
        ConfirmationOfStock = 9,
        CreditRisk = 10,
    }


    public enum JobRequestStatusEnum
    {
        approved = 3,
        cancel = 5,
        disapproved = 4,
        pending = 1,
        processing = 2,

    }

    public enum JobSubTypeClassEnum
    {
        CollateralSearch = 1,
        CollateralVerification = 2,
        CollateralCharting = 3,
        AdditionalCharges =  4,
    }

    public enum JobSourcesEnum
    {
        LoanApplicationDetail = 1,
        LoanBookingAndApproval = 2,
        OverdraftBookingAndApproval = 3,
        ContingentLiabilityBookingAndApproval = 4,
        LMSApplication = 5,
        LMSOperationAndApproval = 6,
        CollateralReleaseApproval = 7,
        LoanApplicationCaptureCRMS = 9,
        TranchBooking = 10
    }
}
