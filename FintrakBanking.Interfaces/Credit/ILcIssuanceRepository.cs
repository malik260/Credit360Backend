using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcIssuanceRepository
    {
        #region LCISSUANCE
        List<LcIssuanceApprovalViewModel> SearchLc(string searchString);
        List<LcIssuanceApprovalViewModel> SearchLcLMS(string searchString);

        IEnumerable<LcIssuanceViewModel> GetLcIssuance(int id);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcEnhancementByLcEnhancementId(int tempLcIssuanceId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuances(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForEnhancement(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForExtension(int staffId);

        IEnumerable<CamProcessedLoanViewModel> GetIFFLinesForLCByCustomerId(int CustomerId, int companyId, int staffId, int branchId);

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForRelease(int staffId);
        IEnumerable<LcReleaseAmountViewModel> GetReleasesForLcIssuance(int lcIssuanceId);

        LcReleaseAmountViewModel AddLCReleaseAmount(LcReleaseAmountViewModel entity);

        LcReleaseAmountViewModel UpdateLCReleaseAmount(LcReleaseAmountViewModel entity);

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForApproval(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForEnhancementApproval(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForExtensionApproval(int staffId);
        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForCancelationApproval(int staffId);

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForReleaseApproval(int staffId);

        //IEnumerable<LcIssuanceViewModel> GetLcIssuanceByIssuanceId(int id);

        LcIssuanceViewModel AddLcIssuance(LcIssuanceViewModel model);
        bool UpdateLcIssuance(LcIssuanceViewModel model, int id, UserInfo user);
        bool DeleteLcIssuance(int id, UserInfo user);

        LcIssuanceViewModel AddLcEnhancement(LcIssuanceViewModel model);
        bool UpdateLcEnhancement(LcIssuanceViewModel model, int id, UserInfo user);
        bool DeleteLcEnhancement(int id, UserInfo user);

        LcIssuanceViewModel AddLcExtension(LcIssuanceViewModel model);
        bool UpdateLcExtension(LcIssuanceViewModel model, int id, UserInfo user);
        bool DeleteLcExtension(int id, UserInfo user);

        bool AddLcArchive(int LcIssuanceId, int operationId);
        bool UpdateOldLcWithEnhancement(int tempLcIssuanceId);

        #endregion LCISSUANCE

        //#region LCDOCUMENT
        //LcDocumentViewModel GetLcDocument(int id);

        //IEnumerable<LcDocumentViewModel> GetLcDocuments();

        //bool AddLcDocument(LcDocumentViewModel model);

        //bool UpdateLcDocument(LcDocumentViewModel model, int id);

        //bool DeleteLcDocument(int id);
        //#endregion LCDOCUMENT

        //#region SHIPPING
        //LcShippingViewModel GetLcShipping(int id);

        //IEnumerable<LcShippingViewModel> GetLcShippings();

        //bool AddLcShipping(LcShippingViewModel model);

        //bool UpdateLcShipping(LcShippingViewModel model, int id);

        //bool DeleteLcShipping(int id);
        //#endregion SHIPPING

        //#region LCCONDITIONS
        //LcConditionViewModel GetLcCondition(int id);

        //IEnumerable<LcConditionViewModel> GetLcConditions();

        //bool AddLcCondition(LcConditionViewModel model);

        //bool UpdateLcCondition(LcConditionViewModel model, int id);

        //bool DeleteLcCondition(int id);
        //#endregion LCCONDITIONS
    }
}
