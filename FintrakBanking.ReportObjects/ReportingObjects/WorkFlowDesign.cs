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
                var approvalTrail = (from f in context.tbl_Approval_Trail
                                     where f.TargetId == targetId && f.OperationId == operationId && f.CompanyId == companyId
                                     orderby f.tbl_Approval_Level.tbl_Approval_Group.GroupId, f.tbl_Approval_Level.ApprovalLevelId
                                     select new WorkflowTrackerViewModel()
                                     {
                                         companyName = company.Name ,
                                         groupName = f.tbl_Approval_Level.tbl_Approval_Group.GroupName,
                                         responseApprovalLevel = f.tbl_Approval_Level.LevelName,
                                         operationName = f.tbl_Operations.OperationName,
                                         arrivalDate = f.SystemArrivalDateTime,
                                         sla = f.tbl_Approval_Level.SLAInterval,
                                         responseDate = (DateTime)(f.SystemResponseDateTime == null ? DateTime.Now : f.SystemResponseDateTime),
                                         comment = f.Comment,
                                         TargetId = f.TargetId,
                                         requestApprovalLevel = (Int64)((Int32?)f.FromApprovalLevelId ?? (Int32?)0) == 0 ? "Undefined Level Initiation" : (Int64)((Int32?)f.FromApprovalLevelId ?? (Int32?)0) > 0 ? ((from m in context.tbl_Approval_Level where m.ApprovalLevelId == f.FromApprovalLevelId select new { m.LevelName }).FirstOrDefault().LevelName) : null,
                                         approvalStatus = ((from n in context.tbl_Approval_Status where n.ApprovalStatusId == f.ApprovalStatusId select new { n.ApprovalStatusName }).FirstOrDefault().ApprovalStatusName)
                                     }).ToList();
                return approvalTrail;

            }
        }



        public static List<WorkFlowViewModel> GetWorkFlowDefination(int companyId, int operationId)
        {
            List<WorkFlowViewModel> data = new List<WorkFlowViewModel>();


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                 data = (from a in context.tbl_Approval_Group
                            join b in context.tbl_Approval_Group_Mapping on a.GroupId equals b.GroupId
                            join c in context.tbl_Approval_Level_Staff on a.GroupId equals c.tbl_Approval_Level.GroupId
                            where c.tbl_Approval_Level.IsActive == true && a.CompanyId == companyId && b.OperationId == operationId
                            group new { a,b,c, c.tbl_Approval_Level, c.tbl_Staff } by  new
                            {
                                LevelStaff = c,
                                Staff = c.tbl_Staff,
                                Level =   c.tbl_Approval_Level,
                                b.tbl_Operations.OperationName,
                                a.GroupName,
                                b.Position ,
                                c.VetoPower
                            } into g

                         orderby g.Key.Level.Position  
                         select new WorkFlowViewModel()
                            {   
                                operationName = g.Key.OperationName,
                                groupName = g.Key.GroupName,
                                vetoPower = g.Key.VetoPower == true ? "Yes" : "No",
                                levelName = g.Key.Level .LevelName,
                                username = (g.Key.Staff.FirstName  + " " + g.Key.Staff.LastName).ToUpper(),
                                scope = g.Key.LevelStaff.ProcessViewScopeId == 1 ? "Default" : g.Key.LevelStaff.ProcessViewScopeId == 2 ? "Group" : g.Key.LevelStaff.ProcessViewScopeId == 3 ? "Global" : null,
                                grpPosition = g.Key.Level.Position.ToString(),
                                levelPosition = g.Key.Level .Position.ToString(),
                                canApprove = g.Key.LevelStaff.CanApprove == true ? "Yes" : "No",
                                canEdit = g.Key.LevelStaff.CanEdit == true ? "Yes" : "No",
                                canUploadFile = g.Key.LevelStaff.CanUploadFile == true ? "Yes" : "No",
                                canSendJobRequest = g.Key.LevelStaff.CanSendJobRequest == true ? "Yes" : "No",

                                staffLevelId = g.Key.LevelStaff.StaffLevelId.ToString()
                            }).ToList();

                return data;


            }
        }


    }
}
 

        
    

