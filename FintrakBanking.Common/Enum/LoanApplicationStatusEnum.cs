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
        CancellationCompleted =22
    }
}
