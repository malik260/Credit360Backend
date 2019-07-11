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

        LcIssuanceViewModel GetLcIssuance(int id);

        IEnumerable<LcIssuanceViewModel> GetLcIssuances();

        IEnumerable<CamProcessedLoanViewModel> GetIFFLinesForLCByCustomerId(int CustomerId, int companyId, int staffId, int branchId);

        IEnumerable<LcIssuanceViewModel> GetLcIssuancesForRelease();

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForApproval(int staffId);

        IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForReleaseApproval(int staffId);

        //IEnumerable<LcIssuanceViewModel> GetLcIssuanceByIssuanceId(int id);

        LcIssuanceViewModel AddLcIssuance(LcIssuanceViewModel model);

        bool UpdateLcIssuance(LcIssuanceViewModel model, int id, UserInfo user);

        bool DeleteLcIssuance(int id, UserInfo user);

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
