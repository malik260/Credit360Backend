
using FintrakBanking.ViewModels.CASA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.CASA
{
   public  interface IOverRideRepository
    {
        bool AddOverRideRequest(IEnumerable<OverrideDetailVeiwModel> entity);
        bool ApproveOverride(ApproveOverrideVeiwModel entity);
      //  bool ApproveOverRideRequest(OverrideDetailVeiwModel entity);
        bool DeleteOverRideRequest(OverrideDetailVeiwModel entity);
        IEnumerable<OverrideItemVeiwModel> GetAllOverRideItems();
        IEnumerable<OverrideDetailVeiwModel> GetAllOverRideRequest();
        OverrideDetailVeiwModel GetOverRideRequestById(int id);
        IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByOverRideItemsId(int id);
        IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByReferenceNumber(string refNo);
        bool UpdateOverRideRequest(OverrideDetailVeiwModel entity);
        int EffectOverride(string customerCode, int overrideItemId, string sourceRef);
        IEnumerable<OverrideDetailVeiwModel> GetOverrideAwaitingApproval(int staffId);
    }

  
}
