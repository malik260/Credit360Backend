using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.Repositories.CASA
{

   public  class OverRideRepository : IOverRideRepository 
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;

        public OverRideRepository(IAuditTrailRepository _auditTrail, IWorkflow _workFlow, IGeneralSetupRepository _genSetup,FinTrakBankingContext _context)
        {
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;          
            this.workFlow = _workFlow;
        }

        public bool AddOverRideRequest(IEnumerable<OverrideDetailVeiwModel> entity)
        {
            var data = entity.Select(c => new TBL_OVERRIDE_DETAIL
            {
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                CREATEDBY = c.createdBy,
                CUSTOMERID = c.customerId,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                ISUSED = false,
                OVERRIDE_ITEMID = c.overrideItemId,
                SOURCE_REFERENCE_NUMBER = c.sourceReferenceNumber
            });
            context.TBL_OVERRIDE_DETAIL.AddRange(data);

            return context.SaveChanges() > 0;

        }

        public bool ApproveOverRideRequest(OverrideDetailVeiwModel entity)
        {
            var data = context.TBL_OVERRIDE_DETAIL.Where(c => c.OVERRIDE_DETAILID == entity.overrideDetailId).FirstOrDefault();
            data.OVERRIDE_DETAILID = entity.overrideDetailId;
            return  context.SaveChanges() > 0;
        }

        public bool DeleteOverRideRequest(OverrideDetailVeiwModel entity)
        {
            var data = context.TBL_OVERRIDE_DETAIL.Where(c => c.OVERRIDE_DETAILID == entity.overrideDetailId);
             
             context.TBL_OVERRIDE_DETAIL .RemoveRange(data);
          return   context.SaveChanges() > 0;
        }

        public IEnumerable<OverrideItemVeiwModel> GetAllOverRideItems()
        {
            var data = context.TBL_OVERRIDE_ITEM.Select(c => new OverrideItemVeiwModel
            {
               itemId = c.OVERRIDE_ITEMID ,
                itemName = c.OVERIDE_ITEMNAME 
            });

            return data.ToList();

        }

        public IQueryable<OverrideDetailVeiwModel> AllOverRideRequest()
        {
            var data = context.TBL_OVERRIDE_DETAIL.Select(c => new OverrideDetailVeiwModel
            {
                approvedStatusId = c.APPROVALSTATUSID,
                createdBy = c.CREATEDBY,
                customerId = c.CUSTOMERID,
                dateTimeCreated = c.DATETIMECREATED,
                isUsed = c.ISUSED,
                overrideDetailId = c.OVERRIDE_ITEMID,
                overrideItemId = c.OVERRIDE_ITEMID,
                sourceReferenceNumber = c.SOURCE_REFERENCE_NUMBER
            });

            return data;
        }

        public IQueryable<OverrideDetailVeiwModel> GetAllOverRideRequest()
        {
            return AllOverRideRequest();
            
        }

        public OverrideDetailVeiwModel GetOverRideRequestById(int id)
        {
           return AllOverRideRequest().Where(e=> e.overrideDetailId == id).FirstOrDefault();
        }

        public IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByOverRideItemsId(int id)
        {
            return AllOverRideRequest().Where(e => e.overrideItemId == id);
        }

        public IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByReferenceNumber(string refNo)
        {
            return AllOverRideRequest().Where(e => e.sourceReferenceNumber == refNo);
        }

        public bool UpdateOverRideRequest(OverrideDetailVeiwModel entity)
        {
            var data = context.TBL_OVERRIDE_DETAIL.Where(c => c.OVERRIDE_DETAILID == entity.overrideDetailId).FirstOrDefault();
            data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
            data.CREATEDBY = entity.createdBy;
            data.CUSTOMERID = entity.customerId;
            data.DATETIMECREATED = genSetup.GetApplicationDate();
            data.ISUSED = entity.isUsed;
            data.OVERRIDE_ITEMID = entity.overrideItemId;
            data.SOURCE_REFERENCE_NUMBER = entity.sourceReferenceNumber;             

            return context.SaveChanges() > 0;
        }
    }
}
