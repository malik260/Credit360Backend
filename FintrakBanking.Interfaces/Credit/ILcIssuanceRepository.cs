using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Interfaces.credit
{
    public interface ILcIssuanceRepository
    {
        #region LCISSUANCE
        LcIssuanceViewModel GetLcIssuance(int id);

        IEnumerable<LcIssuanceViewModel> GetLcIssuances();

        //IEnumerable<LcIssuanceViewModel> GetLcIssuanceByIssuanceId(int id);

        bool AddLcIssuance(LcIssuanceViewModel model);

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
