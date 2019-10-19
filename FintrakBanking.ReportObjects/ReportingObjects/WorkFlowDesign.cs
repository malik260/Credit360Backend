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

        public  IEnumerable<WorkflowTrackerViewModel> TrackWorkFlow(int operationId, int companyId, int targetId, int staffId)
        {


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var staffSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();
                var approvalTrail = (from f in context.TBL_APPROVAL_TRAIL
                                     join l in context.TBL_APPROVAL_LEVEL on f.FROMAPPROVALLEVELID  equals l.APPROVALLEVELID 
                                     join g in context.TBL_APPROVAL_GROUP on l.GROUPID equals g.GROUPID
                                     join b in context.TBL_APPROVAL_GROUP_MAPPING on g.GROUPID equals b.GROUPID
                                     join c in context.TBL_APPROVAL_LEVEL on g.GROUPID equals c.GROUPID
                                     where f.TARGETID == targetId || f.TARGETID != targetId && f.OPERATIONID == operationId && f.COMPANYID == companyId
                                     orderby b.POSITION, c.POSITION

                                     select new WorkflowTrackerViewModel()
                                     {
                                         companyName = company.NAME==null? "": company.NAME,
                                         groupName = l.TBL_APPROVAL_GROUP.GROUPNAME==null ? "": l.TBL_APPROVAL_GROUP.GROUPNAME,
                                         responseApprovalLevel = l.LEVELNAME,
                                         operationName = f.TBL_OPERATIONS.OPERATIONNAME==null ? "": f.TBL_OPERATIONS.OPERATIONNAME,
                                         arrivalDate = f.SYSTEMARRIVALDATETIME,
                                         sla = l.SLAINTERVAL,
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
                            join c in context.TBL_APPROVAL_LEVEL on a.GROUPID equals c.GROUPID
                            //join d in context.TBL_APPROVAL_LEVEL_STAFF on a.GROUPID equals b.TBL_APPROVAL_LEVEL.GROUPID
                         where c.ISACTIVE == true && a.COMPANYID == companyId && b.OPERATIONID == operationId
                         orderby b.POSITION ascending, c.POSITION  ascending                   
                         select new WorkFlowViewModel()
                            {   
                                operationName = b.TBL_OPERATIONS.OPERATIONNAME==null? "": b.TBL_OPERATIONS.OPERATIONNAME,
                                groupName = a.GROUPNAME,
                                //vetoPower = c.VETOPOWER == true ? "Yes" : "No",
                                levelName = c.LEVELNAME,
                                username = "Testing",//(c.TBL_STAFF.FIRSTNAME  + " " + c.TBL_STAFF.LASTNAME).ToUpper(),
                                //scope = c.PROCESSVIEWSCOPEID == 1 ? "Default" : c.PROCESSVIEWSCOPEID == 2 ? "Group" : c.PROCESSVIEWSCOPEID == 3 ? "Global" : null,
                                grpPosition = b.POSITION,
                                levelPosition = c.POSITION,
                                canApprove = c.CANAPPROVE == true ? "Yes" : "No",
                                canEdit = c.CANEDIT == true ? "Yes" : "No",
                                canUploadFile = c.CANUPLOAD == true ? "Yes" : "No",
                                //staffLevelId = c.STAFFLEVELID.ToString()
                            }).ToList();

                return data;


            }
        }


    }
}
 

        
    

