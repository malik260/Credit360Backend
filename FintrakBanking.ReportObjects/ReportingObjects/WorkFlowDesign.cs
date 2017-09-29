using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects
{

    public   class WorkFlowDesign 
    {


        //private static IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int operationId, int companyId)
        //{
        //    IQueryable<WorkflowTrackerViewModel> result;
        //    using (FinTrakBankingContext context = new FinTrakBankingContext())
        //    {
        //        result = (from a in context.tbl_Approval_Trail
        //                  where a.OperationId == operationId && a.CompanyId == companyId
        //                  select

        //                  new WorkflowTrackerViewModel
        //                  {
        //                      arrivalDate = a.ArrivalDate,
        //                      responseApprovalLevel = context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
        //                      responseDate = a.ResponseDate,
        //                      systemArrivalDate = a.SystemArrivalDateTime,
        //                      sla = a.tbl_Approval_Level.SLAInterval,
        //                      systemResponseDate = a.SystemResponseDateTime,
        //                      slaDifference = a.ResponseDate.HasValue == true ? a.ResponseDate.Value.Subtract(a.ArrivalDate).Minutes : 0,
        //                      responseStaffName = !a.ResponseStaffId.HasValue ? "Awaiting Action" : a.tbl_Staff1.FirstName + " " + a.tbl_Staff1.LastName,
        //                      comment = a.Comment,
        //                      requestStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
        //                      requestApprovalLevel = !a.FromApprovalLevelId.HasValue ? "Initiation" : context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
        //                      TargetId = a.TargetId,
        //                      approvalStatus = context.tbl_Approval_Status.FirstOrDefault(c => c.ApprovalStatusId == a.ApprovalStatusId).ApprovalStatusName
        //                  });
        //        return result;
        //    }

        
        //}

       public static IEnumerable<WorkflowTrackerViewModel> TrackWorkFlow(int operationId, int companyId, int targetId)
        {
           

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();
                var result = (from a in context.tbl_Approval_Trail
                              join b in context.tbl_Approval_Level on a.FromApprovalLevelId equals b.ApprovalLevelId
                              join c in context.tbl_Approval_Group_Mapping on b.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().GroupOperationMappingId equals c.GroupOperationMappingId
                              join d in context.tbl_Approval_Group on c.GroupId equals d.GroupId
                              join e in context.tbl_Operations on c.OperationId equals e.OperationId


                              join n in context.tbl_Approval_Level on a.ToApprovalLevelId equals n.ApprovalLevelId
                              join m in context.tbl_Approval_Group_Mapping on n.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().GroupOperationMappingId equals m.GroupOperationMappingId
                              join o in context.tbl_Approval_Group on m.GroupId equals o.GroupId
                              join p in context.tbl_Operations on m.OperationId equals p.OperationId
                             
                              where a.OperationId == operationId && a.CompanyId == companyId && a.TargetId == targetId
                              orderby a.TargetId descending
                              select

                         new WorkflowTrackerViewModel
                         {
                             groupName = d.GroupName,
                             operationName = e.OperationName,
                             companyName = company.Name,
                             arrivalDate = a.SystemArrivalDateTime,

                             responseApprovalLevel = a.ToApprovalLevelId.HasValue ? n.LevelName : "N/A",// context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
                             responseDate = (DateTime)(a.SystemResponseDateTime == null ? DateTime.Now : a.SystemResponseDateTime),
                             responseStaffName = !a.ResponseStaffId.HasValue ? "Awaiting Action" : a.tbl_Staff1.FirstName + " " + a.tbl_Staff1.LastName,


                             sla = b.SLAInterval,

                             comment = a.Comment,
                             requestStaffName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                             requestApprovalLevel = a.FromApprovalLevelId.HasValue ? b.LevelName : "N/A",// context.tbl_Approval_Level.FirstOrDefault(c => c.ApprovalLevelId == a.FromApprovalLevelId).LevelName,
                             TargetId = a.TargetId,
                             approvalStatus = context.tbl_Approval_Status.FirstOrDefault(c => c.ApprovalStatusId == a.ApprovalStatusId).ApprovalStatusName
                         });
                return result.ToList();
            }
        }

       

        public static List<GroupWorkFlowSetup> GetWorkFlowDefination(int companyId)
        {
             IQueryable<GroupWorkFlowSetup> data;

            using(FinTrakBankingContext context = new FinTrakBankingContext()) {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();
                data = (from a in context.tbl_Approval_Group
                        join b in context.tbl_Approval_Group_Mapping on a.GroupId equals b.GroupId
                        join d in context.tbl_Approval_Level on b.GroupOperationMappingId equals d.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().GroupOperationMappingId
                        join c in context.tbl_Approval_Level_Staff on d.ApprovalLevelId equals c.StaffLevelId
                        where a.CompanyId == companyId
                        orderby (b.Position)
                        select new GroupWorkFlowSetup()
                        {
                            CompanyName = company.Name ,
                            OperationId = b.OperationId,
                            GroupName = a.GroupName,
                            IsBeforeCAMApproval = a.IsBeforeCAMApproval,
                            IsCommittee = a.IsCommittee,
                            CanDoRiskAssessment = d.CanDoRiskAssessment,
                            CanEdit = d.CanEdit,
                            CanOverideAuthorisation = d.CanOverideAuthorisation,
                            CanPerformFinancialAnalysis = d.CanPerformFinancialAnalysis,
                            CanRecieveAdjustment = d.CanRecieveAdjustment,
                            CanRecieveEmail = d.CanRecieveEmail,
                            CanRecieveSMS = d.CanRecieveSMS,
                            CanRouteBack = d.CanRouteBack,
                            HasChecklist = d.HasChecklist,
                            IsActive = d.IsActive,
                            IsPoliticallyExposed = d.IsPoliticallyExposed,
                            LevelName = d.LevelName,
                            MinimumAmount = d.MaximumAmount,
                            NumberOfApprovals = d.NumberOfApprovals,
                            NumberOfUsers = d.NumberOfUsers,
                            RequireAuthorisation = d.RequireAuthorisation,
                            RouteViaStaffOrganogram = d.RouteViaStaffOrganogram,
                            SLAInterval = d.SLAInterval,
                            VetoPower = c.VetoPower,
                            CanApprove = c.CanApprove,
                            CanSendJobRequest = c.CanSendJobRequest,
                            CanUploadFile = c.CanUploadFile,
                            CanViewApproval = c.CanViewApproval,
                            CanViewCAMDocument = c.CanViewCAMDocument,
                            CanViewUploadedFile = c.CanViewUploadedFile,
                            MaximumAmount = c.MaximumAmount,
                            StaffName = context.tbl_Staff.Where(t => t.StaffId == c.StaffId).Select(t => t.FirstName + " " + t.LastName).FirstOrDefault()
                        }).AsQueryable();
            }
            return data.ToList();

        }

        public static GroupWorkFlowSetup GetWorkFlowDefinationByOperation(int operationId, int companyId)
        {
            var data = GetWorkFlowDefination(companyId).Where(c => c.OperationId == operationId).FirstOrDefault ();
            return data;
        }
         
    }
}
