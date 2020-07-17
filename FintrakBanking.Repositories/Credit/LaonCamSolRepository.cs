using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using GemBox.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class LaonCamSolRepository : ILaonCamSolRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;
        private IGeneralSetupRepository genSetup;

        public LaonCamSolRepository(FinTrakBankingContext _context, IGeneralSetupRepository _generalSetup, IAuditTrailRepository _auditTrail, IWorkflow _workflow,
            IGeneralSetupRepository _genSetup)
        {
            context = _context;
            generalSetup = _generalSetup;
            auditTrail = _auditTrail;
            workflow = _workflow;
            genSetup = _genSetup;
        }

        public List<LoanCAMSOLViewModel> GetCamSol()
        {
            var data = (from camsol in context.TBL_LOAN_CAMSOL
                            //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                        join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                        select new LoanCAMSOLViewModel
                        {
                            accountname = camsol.ACCOUNTNAME,
                            accountnumber = camsol.ACCOUNTNUMBER,
                            balance = camsol.BALANCE,
                            camsoltypeid = camsol.CAMSOLTYPEID,
                            cantakeloan = camsol.CANTAKELOAN,
                            customercode = camsol.CUSTOMERCODE,
                            customername = camsol.CUSTOMERNAME,
                            date = camsol.DATE,
                            camsolType = c.CAMSOLTYPENAME,
                            loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                            interestinsuspense = camsol.INTERESTINSUSPENSE,
                            loancamsolid = camsol.LOAN_CAMSOLID,
                            loanid = camsol.LOANID,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,
                        });
            return data.ToList();
        }

        public List<LoanCAMSOLViewModel> GetCamSol(string loancamsolid)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       where camsol.CUSTOMERNAME.ToLower().Contains(loancamsolid.ToLower()) || camsol.CUSTOMERCODE == loancamsolid
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           camsolType = c.CAMSOLTYPENAME,
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                       };

            var amconData = (from camsol in context.TBL_TEMP_LOAN_CAMSOL
                            join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                            where (camsol.CUSTOMERNAME.ToLower().Contains(loancamsolid.ToLower()) || camsol.CUSTOMERCODE == loancamsolid) 
                                && camsol.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && camsol.CAMSOLTYPEID == (int)CamsolTypeEnum.AmconList
                            select new LoanCAMSOLViewModel
                            {
                                accountname = camsol.ACCOUNTNAME,
                                accountnumber = camsol.ACCOUNTNAME,
                                balance = camsol.BALANCE,
                                camsoltypeid = camsol.CAMSOLTYPEID,
                                cantakeloan = camsol.CANTAKELOAN,
                                customercode = camsol.CUSTOMERCODE,
                                customername = camsol.CUSTOMERNAME,
                                camsolType = c.CAMSOLTYPENAME,
                                loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                                date = camsol.DATE,
                                interestinsuspense = camsol.INTERESTINSUSPENSE,
                                loancamsolid = camsol.TEMPLOAN_CAMSOLID,
                                loanid = camsol.LOANID,
                                principal = camsol.PRINCIPAL,
                                remark = camsol.REMARK,
                            }).OrderByDescending(O => O.loancamsolid);

            return data.Union(amconData).ToList();
        }
        public List<LoanCAMSOLViewModel> CamSolAwaitingApproval(int companyId, int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CamsolBackbookModification).ToList();

            var data = from camsol in context.TBL_TEMP_LOAN_CAMSOL
                       join atrail in context.TBL_APPROVAL_TRAIL on camsol.TEMPLOAN_CAMSOLID equals atrail.TARGETID
                       where atrail.RESPONSESTAFFID == null
                             && atrail.OPERATIONID == (int)OperationsEnum.CamsolBackbookModification
                             && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                             && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                       orderby camsol.TEMPLOAN_CAMSOLID descending
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           camsolType = context.TBL_LOAN_CAMSOL_TYPE.Where(o => o.CAMSOLTYPEID == camsol.CAMSOLTYPEID).Select(o => o.CAMSOLTYPENAME).FirstOrDefault(),
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                           tempLoancamsolid = (short)camsol.TEMPLOAN_CAMSOLID
                       };
            return data.ToList();
        }
        public List<LoanCAMSOLViewModel> GetCamSolByCustomerCode(string customerCode)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.CUSTOMERCODE == customerCode
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           date = camsol.DATE,
                           camsolType = c.CAMSOLTYPENAME,
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,

                       };
            return data.ToList();
        }
        public List<LoanCAMSOLViewModel> GetCamSolByType(int camsolTyepId)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                           //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.CAMSOLTYPEID == camsolTyepId
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           date = camsol.DATE,
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           camsolType = c.CAMSOLTYPENAME,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                       };
            return data.ToList();
        }

        public List<LoanCAMSOLViewModel> GetCamSolType()
        {
            var camsolType = (from x in context.TBL_LOAN_CAMSOL_TYPE
                              select new LoanCAMSOLViewModel
                              {
                                  camsoltypeid = x.CAMSOLTYPEID,
                                  camsolType = x.CAMSOLTYPENAME
                              }).ToList();
            return camsolType;
        }

        public LoanCAMSOLViewModel ViewCamSolByType(int id)
        {
            var data = from camsol in context.TBL_LOAN_CAMSOL
                           //join a in context.TBL_LOAN_SYSTEM_TYPE on camsol.LOANSYSTEMTYPEID equals a.LOANSYSTEMTYPEID
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.LOAN_CAMSOLID == id
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           camsolType = c.CAMSOLTYPENAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,

                       };
            return data.FirstOrDefault();
        }

        public LoanCAMSOLViewModel CamSolAwaitingApprovalById(int id)
        {
            var data = from camsol in context.TBL_TEMP_LOAN_CAMSOL
                       join c in context.TBL_LOAN_CAMSOL_TYPE on camsol.CAMSOLTYPEID equals c.CAMSOLTYPEID
                       where camsol.TEMPLOAN_CAMSOLID == id
                       select new LoanCAMSOLViewModel
                       {
                           accountname = camsol.ACCOUNTNAME,
                           accountnumber = camsol.ACCOUNTNAME,
                           balance = camsol.BALANCE,
                           camsoltypeid = camsol.CAMSOLTYPEID,
                           cantakeloan = camsol.CANTAKELOAN,
                           customercode = camsol.CUSTOMERCODE,
                           customername = camsol.CUSTOMERNAME,
                           loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == camsol.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                           camsolType = c.CAMSOLTYPENAME,
                           date = camsol.DATE,
                           interestinsuspense = camsol.INTERESTINSUSPENSE,
                           loancamsolid = camsol.LOAN_CAMSOLID,
                           loanid = camsol.LOANID,
                           principal = camsol.PRINCIPAL,
                           remark = camsol.REMARK,
                           tempLoancamsolid = (short)camsol.TEMPLOAN_CAMSOLID
                       };

            return data.FirstOrDefault();
        }


        public string ApproveCamsol(LoanCAMSOLViewModel option)
        {

            var data = (from camsol in context.TBL_LOAN_CAMSOL
                        where camsol.CUSTOMERCODE == option.customercode
                        select camsol).ToList();

            if (data != null)
            {
                string listOfExistingCamsols = string.Empty;
                foreach (var x in data)
                {
                    var iSCamsolExit = context.TBL_TEMP_LOAN_CAMSOL.Any(a => a.LOAN_CAMSOLID == x.LOAN_CAMSOLID && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved);

                    bool cantakeLoanStatus = option.updateOption;

                    if (!iSCamsolExit)
                    {
                        var temp = new TBL_TEMP_LOAN_CAMSOL
                        {
                            ACCOUNTNAME = x.ACCOUNTNAME,
                            ACCOUNTNUMBER = x.ACCOUNTNUMBER,
                            APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                            BALANCE = x.BALANCE,
                            CREATEDBY = x.CREATEDBY,
                            CAMSOLTYPEID = x.CAMSOLTYPEID,
                            CANTAKELOAN = cantakeLoanStatus,
                            COMPANYID = x.COMPANYID,
                            CUSTOMERCODE = x.CUSTOMERCODE,
                            CUSTOMERNAME = x.CUSTOMERNAME,
                            DATE = x.DATE,
                            DATETIMECREATED = DateTime.Now,
                            INTERESTINSUSPENSE = x.INTERESTINSUSPENSE,
                            ISCURRENT = true,

                            LOANID = x.LOANID,
                            LOANSYSTEMTYPEID = x.LOANSYSTEMTYPEID,
                            LOAN_CAMSOLID = x.LOAN_CAMSOLID,
                            PRINCIPAL = x.PRINCIPAL,
                            REMARK = x.REMARK,
                        };

                        context.TBL_TEMP_LOAN_CAMSOL.Add(temp);
                        context.SaveChanges();

                        workflow.StaffId = x.CREATEDBY;
                        workflow.CompanyId = x.COMPANYID;
                        workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                        workflow.TargetId = temp.TEMPLOAN_CAMSOLID;
                        workflow.Comment = "Request for CAMSOL/Blackbook approval";
                        workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
                        workflow.DeferredExecution = false;
                        workflow.ExternalInitialization = true;
                        workflow.LogActivity();
                    }
                    else
                    {
                        listOfExistingCamsols = listOfExistingCamsols + x.CUSTOMERCODE + " | ";
                    }
                }

                if (listOfExistingCamsols != null)
                {
                    return " The following Customer code is currently undergoing approval : " + listOfExistingCamsols;
                }

            }
            return " Record not found ";
        }

        private bool finalCamsolApproval(LoanCAMSOLViewModel data, short StatusId)
        {
            var tempCamsol = (from x in context.TBL_TEMP_LOAN_CAMSOL
                               where x.CUSTOMERCODE == data.customercode
                               orderby x.TEMPLOAN_CAMSOLID descending
                               select new { x.TEMPLOAN_CAMSOLID, x.LOAN_CAMSOLID, x.CANTAKELOAN, x.APPROVALSTATUSID, x.CUSTOMERCODE }).FirstOrDefault();

            if (tempCamsol != null)
            {
                context.TBL_LOAN_CAMSOL.Where(o => o.CUSTOMERCODE == tempCamsol.CUSTOMERCODE).ToList().ForEach(x =>
                {
                    x.CANTAKELOAN = tempCamsol.CANTAKELOAN;
                });

                context.TBL_TEMP_LOAN_CAMSOL.Where(o => o.CUSTOMERCODE == data.customercode).ToList().ForEach(x =>
                {
                    x.APPROVALSTATUSID = (short) ApprovalStatusEnum.Approved;
                });

                return tempCamsol.CANTAKELOAN;
            }

            return false;
        }



        public camsolBulkFeedbackViewModel UploadCamsolData(CamsolDocumentViewModel model, byte[] file)
        {
            var camsolInfo = new List<LoanCAMSOLViewModel>();
            var failedCamsolInfo = new List<LoanCAMSOLViewModel>();
            var camsolBulkFeedbackViewModel = new camsolBulkFeedbackViewModel();

            // Loads a spreadsheet from a file with the specified path
            //Limited unlicenced key : SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY"); 
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5");
            MemoryStream ms = new MemoryStream(file);
            ExcelFile ef = ExcelFile.Load(ms, LoadOptions.XlsxDefault);

            //ExcelWorksheet ws = ef.Worksheets.ActiveWorksheet;
            ExcelWorksheet ws = ef.Worksheets[0]; //.ActiveWorksheet;
            CellRange range = ef.Worksheets.ActiveWorksheet.GetUsedCellRange(true);

            for (int j = range.FirstRowIndex; j <= range.LastRowIndex; j++)
            {
                var rowSuccess = true;
                int excelRowPosition = 1;
                LoanCAMSOLViewModel camsolRowData = new LoanCAMSOLViewModel();

                for (int i = range.FirstColumnIndex; i <= range.LastColumnIndex; i++)
                {
                    ExcelCell cell = range[j - range.FirstRowIndex, i - range.FirstColumnIndex];

                    string cellName = CellRange.RowColumnToPosition(j, i);
                    string cellRow = ExcelRowCollection.RowIndexToName(j);
                    string cellColumn = ExcelColumnCollection.ColumnIndexToName(i);
                    excelRowPosition = Convert.ToInt32(cellRow);
                    if (Convert.ToInt32(cellRow) == 1) continue;

                    switch (cellColumn)
                    {
                        case "A":
                            camsolRowData.date = DateTime.Now;
                            camsolRowData.customercode = cell.Value.ToString();

                            if (camsolRowData.customercode == null)
                            {
                                rowSuccess = false;
                                camsolRowData.message = camsolRowData.message + $"Customer Code cannot be null. ";
                            }
                            break;
                        case "B":
                            var balance = cell.Value.ToString();
                            camsolRowData.balance = Convert.ToDecimal(balance);

                            break;
                        case "C":
                            camsolRowData.customername = cell.Value.ToString();
                            if (camsolRowData.customername == null)
                            {
                                rowSuccess = false;
                                camsolRowData.message = camsolRowData.message + $"customername cannot be null. ";
                            }
                            break;
                        case "D":
                            var principal = cell.Value.ToString();
                            camsolRowData.principal = Convert.ToDecimal(principal);

                            break;
                        case "E":
                            var interestinsuspense = cell.Value.ToString();
                            camsolRowData.interestinsuspense = Convert.ToDecimal(interestinsuspense);
                            break;
                        case "F":
                            var accountnumber = "";
                            if (cell.Value != null)
                            {
                                camsolRowData.accountnumber = cell.Value.ToString();
                            }
                            else
                            {
                                camsolRowData.accountnumber = accountnumber;
                            }
                            break;
                        case "G":
                            var accountname = "";
                            if (cell.Value != null)
                            {
                                camsolRowData.accountname = cell.Value.ToString();
                            }
                            else
                            {
                                camsolRowData.accountname = accountname;
                            }

                            break;
                        case "H":
                            var remark = "";
                            if (cell.Value != null)
                            {
                                camsolRowData.remark = cell.Value.ToString();
                            }
                            else
                            {
                                camsolRowData.remark = remark;
                            }
                            break;
                        case "I":
                            var camsolType = context.TBL_LOAN_CAMSOL_TYPE.Find((int) cell.Value);

                            if (camsolType == null)
                            {
                                rowSuccess = false;
                                camsolRowData.message = camsolRowData.message + $"Camsol Type Id Doesnt Exist in the System";
                            }
                            else
                            {
                                camsolRowData.camsolType = camsolType.CAMSOLTYPENAME;
                                camsolRowData.camsoltypeid = (int)cell.Value;
                            }
                            break;
                        case "J":
                            var cantakeloan = cell.Value.ToString();
                            if (cantakeloan == "1")
                            {
                                camsolRowData.cantakeloan = true;
                            }
                            else
                            {
                                camsolRowData.cantakeloan = false;
                            }

                            break;
                    }
                }

                if (rowSuccess && excelRowPosition > 1)
                {
                    camsolRowData.companyId = model.companyId;
                    camsolRowData.message = "Success";
                    camsolInfo.Add(camsolRowData);
                }
                else if (!rowSuccess && excelRowPosition > 1) failedCamsolInfo.Add(camsolRowData);

            };
            if (camsolInfo.Count() < 1)
            {
                camsolBulkFeedbackViewModel.commitedRows = camsolInfo;
                camsolBulkFeedbackViewModel.discardedRows = failedCamsolInfo;
            }
            else
            {
                foreach (var camsolInfoRow in camsolInfo)
                {
                    camsolInfoRow.createdBy = model.createdBy;
                    camsolInfoRow.companyId = model.companyId;
                    camsolInfoRow.BranchId = model.userBranchId;
                    camsolInfoRow.applicationUrl = model.applicationUrl;
                    camsolInfoRow.userIPAddress = model.userIPAddress;
                    camsolInfoRow.applicationUrl = model.applicationUrl;

                    camsolBulkFeedbackViewModel.commitedRows = camsolInfo;
                    camsolBulkFeedbackViewModel.discardedRows = failedCamsolInfo;

                    var response = AddSimpleTempCamsol(camsolInfoRow);

                    if (!response)
                    {
                        camsolBulkFeedbackViewModel.discardedRows.Add(camsolInfoRow);
                        camsolBulkFeedbackViewModel.commitedRows.Remove(camsolInfoRow);
                        camsolBulkFeedbackViewModel.failureCount = camsolBulkFeedbackViewModel.failureCount + 1;
                        camsolBulkFeedbackViewModel.successCount = camsolBulkFeedbackViewModel.successCount - 1;
                    }

                };
            }

            return camsolBulkFeedbackViewModel;
        }
        public bool AddSimpleTempCamsol(LoanCAMSOLViewModel camsolModel)
        {
            //var staffCode = StaticHelpers.GetUniqueKey(6);
            var camsol = new TBL_TEMP_LOAN_CAMSOL()
            {
                CUSTOMERCODE = camsolModel.customercode,
                LOANID = camsolModel.loanid,
                COMPANYID = camsolModel.companyId,
                BALANCE = camsolModel.balance,
                ACCOUNTNAME = camsolModel.accountname,
                ACCOUNTNUMBER = camsolModel.accountnumber,
                REMARK = camsolModel.remark,

                //STAFFCODE = staffCode,
                LOANSYSTEMTYPEID = camsolModel.LOANSYSTEMTYPEID,
                CUSTOMERNAME = camsolModel.customername,
                PRINCIPAL = camsolModel.principal,
                INTERESTINSUSPENSE = camsolModel.interestinsuspense,
                CAMSOLTYPEID = camsolModel.camsoltypeid,
                CANTAKELOAN = camsolModel.cantakeloan,
                CREATEDBY = camsolModel.createdBy,
                DATE = DateTime.Now,
                DATETIMECREATED = DateTime.Now,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                ISCURRENT = true,
                //TBL_TEMP_PROFILE_USER = userInfo
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = camsolModel.createdBy,
                BRANCHID = (short)camsolModel.BranchId,
                DETAIL = $"Initiated Camsol for '{camsolModel?.customername}' with code'{camsolModel?.customercode}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = camsolModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                  DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            
            };

            auditTrail.AddAuditTrail(audit);
            context.TBL_TEMP_LOAN_CAMSOL.Add(camsol);
            var output = context.SaveChanges() > 0;

            workflow.StaffId = camsolModel.createdBy;
            workflow.CompanyId = camsolModel.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = camsol.TEMPLOAN_CAMSOLID;
            workflow.Comment = "New CAMSOL Creation";
            workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            context.SaveChanges();
            return output;
        }
        public string goForApproval(LoanCAMSOLViewModel data)
        {
           // bool response = false;
            bool action = false;
            using (var transaction = context.Database.BeginTransaction())
            {

                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (short)data.approvalStatusId;
                workflow.TargetId = data.tempLoancamsolid;
                workflow.Comment = data.comment;
                workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (data.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            action = finalCamsolApproval(data, (short)workflow.StatusId);
                        }
                    }

                    if (context.SaveChanges() > 0)
                    {
                        transaction.Commit();

                        if (action == true)
                            return "Consession has been granted to access loan!";
                        else
                            return "This customer has been blacklisted successfully!";
                    }

                    return "Operation could not be completed!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }

            }

        }

        public bool GoForBulkApproval(LoanCAMSOLViewModel data)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.StaffId = data.staffId;
                    workflow.CompanyId = data.companyId;
                    workflow.StatusId = ((short)data.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)data.approvalStatusId;
                    workflow.TargetId = data.tempLoancamsolid;
                    workflow.Comment = data.comment;
                    workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;

                    workflow.LogActivity();

                    var b = workflow.NextLevelId ?? 0;
                    if (b == 0 && workflow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveLoanCamsol(data.tempLoancamsolid, (short)workflow.StatusId, data);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        var tempApprovalLoanCamsol = (from a in context.TBL_TEMP_LOAN_CAMSOL where a.TEMPLOAN_CAMSOLID == data.tempLoancamsolid select a).FirstOrDefault();
                        if (tempApprovalLoanCamsol != null)
                        {
                            tempApprovalLoanCamsol.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                            tempApprovalLoanCamsol.ISCURRENT = true;
                        }
                        context.SaveChanges();
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }




        private bool ApproveLoanCamsol(int tempLoancamsolid, short approvalStatusId, LoanCAMSOLViewModel data)
        {
            //var tempApprovalRelief = (from a in context.TBL_TEMP_STAFF_RELIEF where a.TEMPRELIEFID == targetId select a).FirstOrDefault();
            var tempApprovalLoanCamsol = context.TBL_TEMP_LOAN_CAMSOL.Find(tempLoancamsolid);

            TBL_LOAN_CAMSOL targetApprovalLoanCamsol;
            if (tempApprovalLoanCamsol.LOAN_CAMSOLID > 0)
            {
                targetApprovalLoanCamsol = context.TBL_LOAN_CAMSOL.Find(tempApprovalLoanCamsol.LOAN_CAMSOLID);
                if (targetApprovalLoanCamsol != null)
                {
                    targetApprovalLoanCamsol.CUSTOMERCODE = tempApprovalLoanCamsol.CUSTOMERCODE;
                    targetApprovalLoanCamsol.LOANID = tempApprovalLoanCamsol.LOANID;
                    targetApprovalLoanCamsol.COMPANYID = tempApprovalLoanCamsol.COMPANYID;
                    targetApprovalLoanCamsol.BALANCE = tempApprovalLoanCamsol.BALANCE;
                    //STAFFCODE = staffCode,
                    targetApprovalLoanCamsol.LOANSYSTEMTYPEID = tempApprovalLoanCamsol.LOANSYSTEMTYPEID;
                    targetApprovalLoanCamsol.CUSTOMERNAME = tempApprovalLoanCamsol.CUSTOMERNAME;
                    targetApprovalLoanCamsol.PRINCIPAL = tempApprovalLoanCamsol.PRINCIPAL;
                    targetApprovalLoanCamsol.INTERESTINSUSPENSE = tempApprovalLoanCamsol.INTERESTINSUSPENSE;
                    targetApprovalLoanCamsol.CAMSOLTYPEID = tempApprovalLoanCamsol.CAMSOLTYPEID;
                    targetApprovalLoanCamsol.CANTAKELOAN = tempApprovalLoanCamsol.CANTAKELOAN;
                };
            }
            else
            {
                targetApprovalLoanCamsol = new TBL_LOAN_CAMSOL()
                {
                    CUSTOMERCODE = tempApprovalLoanCamsol.CUSTOMERCODE,
                    LOANID = tempApprovalLoanCamsol.LOANID,
                    COMPANYID = tempApprovalLoanCamsol.COMPANYID,
                    BALANCE = tempApprovalLoanCamsol.BALANCE,
                    //STAFFCODE = staffCode,
                    LOANSYSTEMTYPEID = tempApprovalLoanCamsol.LOANSYSTEMTYPEID,
                    CUSTOMERNAME = tempApprovalLoanCamsol.CUSTOMERNAME,
                    PRINCIPAL = tempApprovalLoanCamsol.PRINCIPAL,
                    INTERESTINSUSPENSE = tempApprovalLoanCamsol.INTERESTINSUSPENSE,
                    CAMSOLTYPEID = tempApprovalLoanCamsol.CAMSOLTYPEID,
                    CANTAKELOAN = tempApprovalLoanCamsol.CANTAKELOAN,
                    CREATEDBY = (int)tempApprovalLoanCamsol.CREATEDBY,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    ACCOUNTNUMBER = tempApprovalLoanCamsol.ACCOUNTNUMBER,
                    ACCOUNTNAME = tempApprovalLoanCamsol.ACCOUNTNAME,
                    REMARK = tempApprovalLoanCamsol.REMARK,
                    DATE = DateTime.Now,

                };
                context.TBL_LOAN_CAMSOL.Add(targetApprovalLoanCamsol);
            }


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CamsolApproval,
                STAFFID = data.staffId,
                BRANCHID = (short)data.BranchId,
                DETAIL = "Approved Loan Camsol",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                 URL = data.applicationUrl,       
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            try
            {
                auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------
                var response = context.SaveChanges() > 0;
                tempApprovalLoanCamsol.LOAN_CAMSOLID = targetApprovalLoanCamsol.LOAN_CAMSOLID;
                tempApprovalLoanCamsol.APPROVALSTATUSID = approvalStatusId;
                tempApprovalLoanCamsol.ISCURRENT = false;

                context.SaveChanges();
                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        //public bool GoForBulkApproval(List<ApprovalViewModel> model, UserInfo userInfo)
        //{
        //    if (model.Count == 0) return false;
        //    bool output = false;
        //    foreach (var entity in model)
        //    {

        //        using (var trans = context.Database.BeginTransaction())
        //        {
        //            try
        //            {
        //                workflow.StaffId = userInfo.staffId;
        //                workflow.CompanyId = userInfo.companyId;
        //                workflow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
        //                workflow.TargetId = entity.targetId;
        //                workflow.Comment = entity.comment;
        //                workflow.OperationId = (int)OperationsEnum.CamsolBackbookModification;
        //                workflow.ExternalInitialization = false;
        //                workflow.DeferredExecution = true;
        //                workflow.LogActivity();

        //                context.SaveChanges();

        //                if (workflow.NewState == (int)ApprovalState.Ended)
        //                {
        //                    var response = ApproveBulkStaff(entity.targetId, (short)workflow.StatusId, userInfo);
        //                    finalCamsolApproval(data, (short)workflow.StatusId);
        //                    if (response)
        //                    {
        //                        trans.Commit();
        //                    }
        //                    output = true;
        //                }
        //                else
        //                {
        //                    trans.Commit();
        //                    output = false;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                trans.Rollback();
        //                throw new SecureException(ex.Message);
        //            }
        //        }
        //    }
        //    return output;
        //}
        //private bool ApproveBulkStaff(int staffid, short approvalStatusId, UserInfo user)
        //{
        //    bool isUpdate = false;
        //    TBL_TEMP_PROFILE_USER tempUser = null;
        //    List<TBL_TEMP_PROFILE_USERGROUP> tempGroup = null;
        //    List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> tempActivities = null;

        //    List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
        //    List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();

        //    tempUser = (from a in context.TBL_TEMP_PROFILE_USER
        //                where a.TEMPSTAFFID == staffid && a.ISCURRENT == true &&
        //                a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
        //                select a).FirstOrDefault();
        //    if (tempUser != null)
        //    {
        //        tempGroup = (from a in context.TBL_TEMP_PROFILE_USERGROUP where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();
        //        tempActivities = (from a in context.TBL_TEMP_PROFILE_ADTN_ACTIVITY where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();

        //        tempUser.ISCURRENT = false;
        //        tempUser.APPROVALSTATUS = true;
        //        tempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

        //        if (tempActivities.Count > 0)
        //        {
        //            foreach (var item in tempActivities)
        //            {
        //                var userActivity = new TBL_PROFILE_ADDITIONALACTIVITY()
        //                {
        //                    ACTIVITYID = item.ACTIVITYID,
        //                    CANADD = false,
        //                    CANEDIT = false,
        //                    CANAPPROVE = false,
        //                    CANDELETE = false,
        //                    CANVIEW = false,
        //                    CREATEDBY = item.CREATEDBY,
        //                    DATETIMECREATED = DateTime.Now,
        //                };
        //                userActivities.Add(userActivity);
        //            }
        //        }

        //        if (tempGroup.Count > 0)
        //        {
        //            foreach (var item in tempGroup)
        //            {
        //                var grpItem = new TBL_PROFILE_USERGROUP()
        //                {
        //                    GROUPID = item.GROUPID,
        //                    APPROVALSTATUS = false,
        //                    DATETIMECREATED = DateTime.Now,
        //                    CREATEDBY = item.CREATEDBY,
        //                };
        //                userGroups.Add(grpItem);
        //            }
        //            foreach (var item in tempGroup)
        //            {
        //                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
        //                item.ISCURRENT = false;
        //                item.DATEAPPROVED = DateTime.Now;
        //            }
        //        }
        //    }

        //    TBL_STAFF entity = null;
        //    TBL_PROFILE_USER targetUser = null;
        //    var temp = context.TBL_TEMP_STAFF.Find(staffid);
        //    if (temp != null)
        //    {
        //        entity = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == temp.STAFFCODE.ToLower());
        //        // Removing existing groups and activities
        //        if (entity != null)
        //        {

        //            targetUser = (from a in context.TBL_PROFILE_USER
        //                          where a.STAFFID == entity.STAFFID
        //                          select a).FirstOrDefault();
        //            if (targetUser != null)
        //            {
        //                var targetGroups = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == targetUser.USERID).ToList();
        //                var targetActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == targetUser.USERID).ToList();
        //                if (targetGroups.Any())
        //                {
        //                    foreach (var item in targetGroups)
        //                    {
        //                        context.TBL_PROFILE_USERGROUP.Remove(item);
        //                    }
        //                }
        //                if (targetActivities.Any())
        //                {
        //                    foreach (var item in targetActivities)
        //                    {
        //                        context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    if (tempUser != null)
        //    {
        //        if (targetUser != null)
        //        {
        //            isUpdate = true;
        //            targetUser.USERNAME = tempUser.USERNAME;
        //            targetUser.PASSWORD = tempUser.PASSWORD;
        //            targetUser.ISFIRSTLOGINATTEMPT = false;
        //            targetUser.ISACTIVE = true;
        //            targetUser.ISLOCKED = false;
        //            targetUser.FAILEDLOGONATTEMPT = 0;
        //            targetUser.SECURITYQUESTION = tempUser.SECURITYQUESTION;
        //            targetUser.SECURITYANSWER = tempUser.SECURITYANSWER;
        //            targetUser.TBL_PROFILE_USERGROUP = userGroups;
        //            targetUser.TBL_PROFILE_ADDITIONALACTIVITY = userActivities;
        //        }
        //        else
        //        {
        //            targetUser = new TBL_PROFILE_USER()
        //            {
        //                USERNAME = tempUser.USERNAME,
        //                PASSWORD = tempUser.PASSWORD,
        //                ISFIRSTLOGINATTEMPT = false,
        //                ISACTIVE = true,
        //                ISLOCKED = false,
        //                FAILEDLOGONATTEMPT = 0,
        //                SECURITYQUESTION = tempUser.SECURITYQUESTION,
        //                SECURITYANSWER = tempUser.SECURITYANSWER,
        //                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
        //                CREATEDBY = tempUser.CREATEDBY,
        //                LASTUPDATEDBY = tempUser.CREATEDBY,
        //                DATETIMECREATED = tempUser.DATETIMECREATED,
        //                APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved,
        //                APPROVALSTATUS = false,
        //                TBL_PROFILE_USERGROUP = userGroups,
        //                TBL_PROFILE_ADDITIONALACTIVITY = userActivities,
        //            };

        //        }
        //    }

        //    if (temp != null)
        //    {
        //        if (entity != null)
        //        {
        //            entity.FIRSTNAME = temp.FIRSTNAME;
        //            entity.COMPANYID = temp.COMPANYID;
        //            entity.MIDDLENAME = temp.MIDDLENAME;
        //            entity.LASTNAME = temp.LASTNAME;
        //            entity.STAFFCODE = temp.STAFFCODE;
        //            entity.JOBTITLEID = temp.JOBTITLEID;
        //            entity.STAFFROLEID = temp.STAFFROLEID;
        //            entity.SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID;
        //            entity.ADDRESS = temp.ADDRESS;
        //            entity.ADDRESSOFNOK = temp.ADDRESSOFNOK;
        //            entity.BRANCHID = temp.BRANCHID;
        //            entity.COMMENT = temp.COMMENT;
        //            entity.CREATEDBY = temp.CREATEDBY;
        //            if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
        //            entity.DATEOFBIRTH = temp.DATEOFBIRTH;
        //            entity.DATETIMEUPDATED = DateTime.Now;
        //            //entity.DEPARTMENTID = temp.DEPARTMENTID;
        //            entity.DEPARTMENTUNITID = temp.DEPARTMENTUNITID;
        //            entity.EMAIL = temp.EMAIL;
        //            entity.EMAILOFNOK = temp.EMAILOFNOK;
        //            entity.GENDER = temp.GENDER;
        //            entity.GENDEROFNOK = temp.GENDEROFNOK;
        //            entity.MISINFOID = temp.MISINFOID;
        //            entity.NAMEOFNOK = temp.NAMEOFNOK;
        //            entity.NOKRELATIONSHIP = temp.NOKRELATIONSHIP;
        //            entity.PHONE = temp.PHONE;
        //            entity.PHONEOFNOK = temp.PHONEOFNOK;
        //            entity.STATEID = temp.STATEID;
        //            entity.CITYID = temp.CITYID;
        //            entity.DELETED = false;
        //            entity.LOAN_LIMIT = temp.LOAN_LIMIT;
        //            entity.WORKSTARTDURATION = temp.WORKSTARTDURATION;
        //            entity.WORKENDDURATION = temp.WORKENDDURATION;
        //        }
        //        else
        //        {
        //            entity = new TBL_STAFF()
        //            {
        //                FIRSTNAME = temp.FIRSTNAME,
        //                MIDDLENAME = temp.MIDDLENAME,
        //                COMPANYID = temp.COMPANYID,
        //                LASTNAME = temp.LASTNAME,
        //                STAFFCODE = temp.STAFFCODE,
        //                JOBTITLEID = temp.JOBTITLEID,
        //                STAFFROLEID = temp.STAFFROLEID,
        //                SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID,
        //                DEPARTMENTUNITID = temp.DEPARTMENTUNITID,
        //                ADDRESS = temp.ADDRESS,
        //                ADDRESSOFNOK = temp.ADDRESSOFNOK,
        //                BRANCHID = temp.BRANCHID,
        //                COMMENT = temp.COMMENT,
        //                CREATEDBY = temp.CREATEDBY,
        //                DATEOFBIRTH = temp.DATEOFBIRTH,
        //                DATETIMECREATED = DateTime.Now,
        //                //DEPARTMENTID = temp.DEPARTMENTID,
        //                EMAIL = temp.EMAIL,
        //                EMAILOFNOK = temp.EMAILOFNOK,
        //                GENDER = temp.GENDER,
        //                GENDEROFNOK = temp.GENDEROFNOK,
        //                MISINFOID = temp.MISINFOID,
        //                NAMEOFNOK = temp.NAMEOFNOK,
        //                NOKRELATIONSHIP = temp.NOKRELATIONSHIP,
        //                PHONE = temp.PHONE,
        //                PHONEOFNOK = temp.PHONEOFNOK,
        //                STATEID = temp.STATEID,
        //                CITYID = temp.CITYID,
        //                LOAN_LIMIT = temp.LOAN_LIMIT,
        //                WORKSTARTDURATION = temp.WORKSTARTDURATION,
        //                WORKENDDURATION = temp.WORKENDDURATION
        //            };
        //            if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
        //            context.TBL_STAFF.Add(entity);
        //        }
        //        temp.ISCURRENT = false;
        //        temp.APPROVALSTATUSID = approvalStatusId;
        //        temp.DATETIMEUPDATED = DateTime.Now;
        //        temp.LASTUPDATEDBY = user.createdBy;
        //    }


        //    // Audit Section ---------------------------
        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.StaffApproved,
        //        STAFFID = user.staffId,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"Approved Staff '{temp?.FIRSTNAME + " " + temp?.LASTNAME}' with staff code'{temp?.STAFFCODE}'",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = genSetup.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    };

        //    try
        //    {
        //        context.TBL_AUDIT.Add(audit);
        //        var output = context.SaveChanges() > 0;

        //        if (tempUser != null && isUpdate == false)
        //        {
        //            targetUser.STAFFID = entity.STAFFID;
        //            context.TBL_PROFILE_USER.Add(targetUser);
        //            return context.SaveChanges() > 0;
        //        }
        //        return output;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new SecureException(ex.Message);
        //    }
        //}

    }


}
