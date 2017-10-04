using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.ReportObjects
{

    public class WorkFlowDesign
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



        public static List<WorkFlowViewModel> GetWorkFlowDefination(int companyId, int operationId)
        {
            List<WorkFlowViewModel> data = new List<WorkFlowViewModel>();


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //var data = (from a in context.tbl_Approval_Level_Staff
                //            where
                //              a.tbl_Approval_Level.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().OperationId == operationId &&
                //              a.tbl_Approval_Level.IsActive == true && a.tbl_Approval_Level.tbl_Approval_Group.CompanyId == companyId
                //            orderby
                //              a.tbl_Approval_Level.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().Position,
                //              a.tbl_Approval_Level.Position,
                //              a.ApprovalLevelId
                //            select new WorkFlowViewModel()
                //            {
                //                operationName = a.tbl_Approval_Level.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().tbl_Operations.OperationName,
                //                groupName = a.tbl_Approval_Level.tbl_Approval_Group.GroupName,
                //                vetoPower = a.VetoPower == true ? "Yes" : "No",
                //                levelName = a.tbl_Approval_Level.LevelName,
                //                username = (a.tbl_Staff.FirstName + "." + a.tbl_Staff.LastName).ToLower(),
                //                scope = a.ProcessViewScopeId == 1 ? "Default" : a.ProcessViewScopeId == 2 ? "Group" : a.ProcessViewScopeId == 3 ? "global" : null,
                //                grpPosition = a.tbl_Approval_Level.tbl_Approval_Group.tbl_Approval_Group_Mapping.FirstOrDefault().Position.ToString(),
                //                levelPosition = a.tbl_Approval_Level.Position.ToString(),
                //                canApprove = a.CanApprove == true ? "Yes" : "No",
                //                canEdit = a.CanEdit == true ? "Yes" : "No",
                //                canUploadFile = a.CanUploadFile == true ? "Yes" : "No",
                //                canSendJobRequest = a.CanSendJobRequest == true ? "Yes" : "No",

                //                staffLevelId = a.StaffLevelId.ToString()
                //            }).ToList();
                return data;


            }
        }


    }
}
 

        
    

