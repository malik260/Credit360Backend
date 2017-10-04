using FintrakBanking.ReportObjects.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Reports
{
   public  interface IReportingObjects
    {
       // List<GroupWorkFlowSetup> GetWorkFlowDefination(int companyId);

        IQueryable<WorkflowTrackerViewModel> TrackWorkFlow(int operationId, int companyId, int targetId);

      //  GroupWorkFlowSetup GetWorkFlowDefinationByOperation(int operationId, int companyId);
    }
}
