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

        public static IEnumerable<WorkflowTrackerViewModel> TrackWorkFlow(int operationId, int companyId, int targetId, int staffId)
        {


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var staffSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();
                var approvalTrail = (from f in context.TBL_APPROVAL_TRAIL
                                     where f.TARGETID == targetId && f.OPERATIONID == operationId && f.COMPANYID == companyId
                                     orderby f.TBL_APPROVAL_LEVEL.TBL_APPROVAL_GROUP.GROUPID, f.TBL_APPROVAL_LEVEL.APPROVALLEVELID

                                     select new WorkflowTrackerViewModel()
                                     {
                                         companyName = company.NAME ,
                                         groupName = f.TBL_APPROVAL_LEVEL.TBL_APPROVAL_GROUP.GROUPNAME,
                                         responseApprovalLevel = f.TBL_APPROVAL_LEVEL.LEVELNAME,
                                         operationName = f.TBL_OPERATIONS.OPERATIONNAME,
                                         arrivalDate = f.SYSTEMARRIVALDATETIME,
                                         sla = f.TBL_APPROVAL_LEVEL.SLAINTERVAL,
                                         responseDate = (DateTime)(f.SYSTEMRESPONSEDATETIME == null ? DateTime.Now : f.SYSTEMRESPONSEDATETIME),
                                         comment = f.COMMENT,
                                         TargetId = f.TARGETID,
                                         requestApprovalLevel = (Int64)((Int32?)f.FROMAPPROVALLEVELID ?? (Int32?)0) == 0 ? "Undefined Level Initiation" : (Int64)((Int32?)f.FROMAPPROVALLEVELID ?? (Int32?)0) > 0 ? ((from m in context.TBL_APPROVAL_LEVEL where m.APPROVALLEVELID == f.FROMAPPROVALLEVELID select new { m.LEVELNAME }).FirstOrDefault().LEVELNAME) : null,
                                         approvalStatus = ((from n in context.TBL_APPROVAL_STATUS where n.APPROVALSTATUSID == f.APPROVALSTATUSID select new { n.APPROVALSTATUSNAME }).FirstOrDefault().APPROVALSTATUSNAME)
                                     }).ToList();
                return approvalTrail;

            }
        }



        public static List<WorkFlowViewModel> GetWorkFlowDefination(int companyId, int operationId)
        {
            List<WorkFlowViewModel> data = new List<WorkFlowViewModel>();


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                 data = (from a in context.TBL_APPROVAL_GROUP
                            join b in context.TBL_APPROVAL_GROUP_MAPPING on a.GROUPID equals b.GROUPID
                            join c in context.TBL_APPROVAL_LEVEL_STAFF on a.GROUPID equals c.TBL_APPROVAL_LEVEL.GROUPID
                            where c.TBL_APPROVAL_LEVEL.ISACTIVE == true && a.COMPANYID == companyId && b.OPERATIONID == operationId
                            group new { a,b,c, c.TBL_APPROVAL_LEVEL , c.TBL_STAFF } by  new
                            {
                                LevelStaff = c,
                                Staff = c.TBL_STAFF,
                                Level =   c.TBL_APPROVAL_LEVEL,
                                b.TBL_OPERATIONS.OPERATIONNAME,
                                a.GROUPNAME,
                                b.POSITION ,
                                c.VETOPOWER
                            } into g

                         orderby g.Key.Level.POSITION  
                         select new WorkFlowViewModel()
                            {   
                                operationName = g.Key.OPERATIONNAME,
                                groupName = g.Key.GROUPNAME,
                                vetoPower = g.Key.VETOPOWER == true ? "Yes" : "No",
                                levelName = g.Key.Level .LEVELNAME,
                                username = (g.Key.Staff.FIRSTNAME  + " " + g.Key.Staff.LASTNAME).ToUpper(),
                                scope = g.Key.LevelStaff.PROCESSVIEWSCOPEID == 1 ? "Default" : g.Key.LevelStaff.PROCESSVIEWSCOPEID == 2 ? "Group" : g.Key.LevelStaff.PROCESSVIEWSCOPEID == 3 ? "Global" : null,
                                grpPosition = g.Key.Level.POSITION,
                                levelPosition = g.Key.Level .POSITION,
                                canApprove = g.Key.LevelStaff.CANAPPROVE == true ? "Yes" : "No",
                                canEdit = g.Key.LevelStaff.CANEDIT == true ? "Yes" : "No",
                                canUploadFile = g.Key.LevelStaff.CANUPLOADFILE == true ? "Yes" : "No",
                                canSendJobRequest = g.Key.LevelStaff.CANSENDJOBREQUEST == true ? "Yes" : "No",

                                staffLevelId = g.Key.LevelStaff.STAFFLEVELID.ToString()
                            }).ToList();

                return data;


            }
        }


    }
}
 

        
    

