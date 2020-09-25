using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum LoanApplicationStatusEnum
    {
        ApplicationInProgress = 1,
        ApplicationCompleted = 2,
        CAMInProgress = 3,
        CAMCompleted = 4,
        OfferLetterGenerationInProgress = 5,
        OfferLetterGenerationCompleted = 6,
        OfferLetterReviewInProgress = 7,
        OfferLetterReviewCompleted = 8,
        AvailmentInProgress = 9,
        AvailmentCompleted = 10,
        LoanBookingInProgress = 11,
        LoanBookingCompleted = 12,
        ChecklistInProgress = 13,
        ChecklistCompleted = 14,
        BookingRequestInitiated = 15,
        BookingRequestCompleted = 16,
        ApplicationUnderReview = 17,
        BondAndGuaranteesInProgress = 18,
        ApplicationRejected = 19,
        OfferLetterRejected = 20,
        CancellationInProgress=21,
        CancellationCompleted =22,
        LcIssuanceCompleted = 24,
        LcShippingReleaseInProgress = 25,
        LcShippingReleaseCompleted = 26,
        lcUssanceInProgress = 27,
        lcUssanceCompleted = 28,
        LcIssuanceInProgress = 29,
        LetterGenerationRequestInProgress = 30,
        LetterGenerationRequestCompleted = 31,
        checklistDeferralInProgress = 32,
        checklistDeferralCompleted = 33,
        collateralSwapInProgress = 34,
        collateralSwapCompleted = 35,
        LcEnhancementInProgress = 36,
        LcEnhancementCompleted = 37,
        ArchiveInProgress = 38,
        ArchiveCompleted = 39,
        CRMSCODECapturingInprogress = 40,
        CreditFillingInProgress =41,
        DocumentUploadInProgress = 41,
        LcIssuanceExtensionInProgress = 43,
        LcIssuanceExtensionCompleted = 44,
        LcUsanceExtensionInProgress = 45,
        LcUsanceExtensionCompleted = 46,
        //AdhocApprovalInProgress =23,
    }
}
