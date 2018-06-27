using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

        public LoanReviewApplicationRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;

            IQueryable<LoanReviewApplicationViewModel> applications = null;
            bool screenCanViewAll = operationId == (int)OperationsEnum.LoanReviewApprovalApplication;

            // get approval levels 
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            // query
            applications = context.TBL_LMSR_APPLICATION
            .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
            .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                alaba => alaba.ab.a.LOANAPPLICATIONID,
                trail => trail.TARGETID,
                (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
            .SelectMany(
                xy => xy.trail.DefaultIfEmpty(),
            (x, trail) => new LoanReviewApplicationViewModel
            {
                //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                approvalState = trail == null ? "Pending" : trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalTrailId = trail == null ? 0 : trail.APPROVALTRAILID,
                currentApprovalLevel = trail == null ? "" : trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                currentApprovalLevelId = trail == null ? 0 : trail.TOAPPROVALLEVELID,
                lastComment = trail == null ? "" : trail.COMMENT,
                toStaffId = trail == null ? 0 : trail.TOSTAFFID,
                applicationDate = x.application.APPLICATIONDATE,
                approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                approvalStatusId = x.application.APPROVALSTATUSID,
                createdBy = x.application.CREATEDBY,
                loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                branchId = x.branch.BRANCHID,
                branchName = x.branch.BRANCHNAME,
                customerId = x.customer.CUSTOMERID,
                customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,

                applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Select(d => new applicationDetails
                {
                    detailId = d.LOANREVIEWAPPLICATIONID,
                    reviewDetails = d.REVIEWDETAILS,
                    operationTypeId = d.LOANSYSTEMTYPEID,
                    operationType = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                    loanId = d.LOANID, 
                })
                .ToList()
            })
            .GroupBy(d => d.loanReviewApplicationId)
            .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanReviewApplicationId);

            //var list = applications.ToList();
            //var count = applications.Count();

            if (screenCanViewAll) { return applications; };

            return applications.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        public SelectListViewModel GetAllSelectList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement || x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
            || x.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME }).OrderBy(o => o.name).ToList();

            return list;
        }

        public bool SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            //var referenceNumber = GenerateReferenceNumber();
            var applicationDate = general.GetApplicationDate();

            var application = context.TBL_LMSR_APPLICATION.Add(new TBL_LMSR_APPLICATION
            {
                APPLICATIONREFERENCENUMBER = "0000012457896325874512",
                COMPANYID = model.companyId,
                CUSTOMERID = model.customerId,
                BRANCHID = model.branchId,
                // CUSTOMERGROUPID = null,
                DISPUTED = false,
                REQUIRECOLLATERAL = false,
                APPLICATIONDATE = applicationDate,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                APPLICATIONSTATUSID = 1, // -------------------------------------------------- REMOVE COLUMN!!
            });

            foreach (var detail in model.applicationDetails)
            {
                context.TBL_LMSR_APPLICATION_DETAIL.Add(new TBL_LMSR_APPLICATION_DETAIL
                {
                    LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                    LOANID = model.loanId,
                    LOANSYSTEMTYPEID = model.productTypeId,/*Term/Disbursed Facility..Overdraft Facility..Contingent Liability*/
                    OPERATIONID = detail.operationTypeId,
                    REVIEWDETAILS = detail.reviewDetails,
                    REPAYMENTTERMS = String.Empty,
                    REPAYMENTSCHEDULE = String.Empty,
                    CUSTOMERID = model.customerId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending, // REMOVE DUPLICATE [STATUSID]
                    CREATEDBY = model.createdBy,
                    BRANCHID = model.branchId,
                    DATECREATED = applicationDate,
                });
            }

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) return false; // this save is necessary to grab targetid

            workflow.NextProcess(model.companyId, model.createdBy, (int)OperationsEnum.LoanReviewApprovalAppraisal, application.LOANAPPLICATIONID, null, "NIL", true, true);

            return context.SaveChanges() > 0;
        }

        private string GenerateReferenceNumber()
        {
            int length = 10;
            string input = "0000000000";
            var appl = context.TBL_LMSR_APPLICATION.OrderByDescending(x => x.LOANAPPLICATIONID).FirstOrDefault();
            if (appl != null) input = appl.APPLICATIONREFERENCENUMBER;
            string output = (int.Parse(input) + 1).ToString().PadLeft(length, '0');
            return output;
        }

        public List<LoanViewModel> LoanSearch(int getCompanyId, SearchViewModel search)
        {
            IQueryable<LoanViewModel> loans = null;

            loans = context.TBL_LOAN.Where(x => x.ISDISBURSED == true).Select(l => new LoanViewModel
            {
                loanId = l.TERMLOANID,
                customerId = l.CUSTOMERID,
                customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                productId = l.PRODUCTID,
                companyId = l.COMPANYID,
                casaAccountId = l.CASAACCOUNTID,
                branchId = l.BRANCHID,
                branchName = l.TBL_BRANCH.BRANCHNAME,
                loanReferenceNumber = l.LOANREFERENCENUMBER,
                //tenor = (l.MaturityDate - l.EffectiveDate).Days, // returning error

                principalFrequencyTypeId = (short)l.PRINCIPALFREQUENCYTYPEID,
                pricipalFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,
                interestFrequencyTypeId = (short)l.INTERESTFREQUENCYTYPEID,
                interestFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,

                principalNumberOfInstallment = l.PRINCIPALNUMBEROFINSTALLMENT,
                interestNumberOfInstallment = l.INTERESTNUMBEROFINSTALLMENT,

                relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                relationshipOfficerName = l.TBL_STAFF.FIRSTNAME + " " + l.TBL_STAFF.MIDDLENAME + " " + l.TBL_STAFF.LASTNAME,
                relationshipManagerId = l.RELATIONSHIPMANAGERID,
                relationshipManagerName = l.TBL_STAFF1.FIRSTNAME + " " + l.TBL_STAFF1.MIDDLENAME + " " + l.TBL_STAFF1.LASTNAME,
                misCode = l.MISCODE,
                teamMiscode = l.TEAMMISCODE,
                interestRate = l.INTERESTRATE,
                effectiveDate = l.EFFECTIVEDATE,
                maturityDate = l.MATURITYDATE,
                bookingDate = l.BOOKINGDATE,
                principalAmount = l.PRINCIPALAMOUNT,
                principalInstallmentLeft = l.PRINCIPALINSTALLMENTLEFT,
                interestInstallmentLeft = l.INTERESTINSTALLMENTLEFT,
                approvalStatusId = l.APPROVALSTATUSID,
                approvedBy = l.APPROVEDBY,
                approverComment = l.APPROVERCOMMENT,
                dateApproved = l.DATEAPPROVED,
                loanStatusId = l.LOANSTATUSID,
                scheduleTypeId = l.SCHEDULETYPEID,
                isDisbursed = l.ISDISBURSED,
                disbursedBy = l.DISBURSEDBY,
                disburserComment = l.DISBURSERCOMMENT,
                disburseDate = l.DISBURSEDATE,

                approvedAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                operationId = l.OPERATIONID,
                operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == l.OPERATIONID).OPERATIONNAME,
                casaAccountNumber = l.TBL_CASA.PRODUCTACCOUNTNAME,
                productAccountName = l.TBL_PRODUCT.PRODUCTNAME,
                subSectorName = l.TBL_SUB_SECTOR.NAME,
                sectorName = l.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                customerGroupId = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                loanTypeId = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                loanTypeName = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                equityContribution = l.EQUITYCONTRIBUTION,
                firstPrincipalPaymentDate = l.FIRSTPRINCIPALPAYMENTDATE ?? DateTime.Now,
                firstInterestPaymentDate = l.FIRSTINTERESTPAYMENTDATE ?? DateTime.Now,
                outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                principalAdditionCount = l.PRINCIPALADDITIONCOUNT ?? 0,
                principalReductionCount = l.PRINCIPALREDUCTIONCOUNT ?? 0,
                fixedPrincipal = l.FIXEDPRINCIPAL,
                profileLoan = l.PROFILELOAN,
                dischargeLetter = l.DISCHARGELETTER,
                suspendInterest = l.SUSPENDINTEREST,
                //customerSensitivityLevelId = l.CUSTOMERSENSITIVITYLEVELID,
                createdBy = l.CREATEDBY,
                dateTimeCreated = l.DATETIMECREATED,
                isCamsol = context.TBL_LOAN_CAMSOL.Any(x => x.LOANID == l.TERMLOANID),
                productName = l.TBL_PRODUCT.PRODUCTNAME
            });

            if (!String.IsNullOrEmpty(search.searchString))
            {
                loans = loans.Where(x =>
                x.customerName.ToLower().Contains(search.searchString.ToLower())
                || x.loanReferenceNumber.ToLower().Contains(search.searchString.ToLower())
                || x.casaAccountNumber.ToLower().Contains(search.searchString.ToLower())
                );
            }

            return loans.ToList();
        }

        public int SaveCam(CamViewModel model)
        {
            string finalAction = "Updated";
            var cam = new TBL_LOAN_REVIEW_APPLICATN_CAM();

            // check of memo exist for level
            var memo = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Where(x =>
                x.LOANREVIEWAPPLICATIONID == model.applicationId
                && x.APPROVALLEVELID == model.approvalLevelId
            );

            // if force new 
            // if null, create new for level
            if (memo.Any() == false || model.createNew == true)
            {
                cam = new TBL_LOAN_REVIEW_APPLICATN_CAM
                {
                    DOCUMENTATION = model.createNew ? "<p></p>" : model.documentation,
                    LOANREVIEWAPPLICATIONID = model.applicationId,
                    APPROVALLEVELID = model.approvalLevelId,
                    CAMREF = model.referenceNumber,
                    COMPANYID = model.companyId, // NN
                    ISCOMPLETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                context.TBL_LOAN_REVIEW_APPLICATN_CAM.Add(cam);
                finalAction = "Added";
            }
            else
            {
                // if exist update for level
                cam = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Find(model.documentationId);
                if (cam == null) cam = memo.OrderByDescending((x => x.LOANREVIEWCAMID)).FirstOrDefault();
                cam.DOCUMENTATION = model.documentation;
                cam.LASTUPDATEDBY = model.lastUpdatedBy;
                cam.DATETIMEUPDATED = general.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = finalAction == "Added" ? (short)AuditTypeEnum.AppraisalMemorandumAdded : (short)AuditTypeEnum.AppraisalMemorandumUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"'{ finalAction }' Appraisal Memorandum Document'{ model.referenceNumber }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0 ? cam.LOANREVIEWCAMID : 0;
        }

        public List<CamViewModel> GetCamDocuments(int applicationId)
        {
            return context.TBL_LOAN_REVIEW_APPLICATN_CAM
                .Where(x => x.LOANREVIEWAPPLICATIONID == applicationId)
                .Select(cam => new CamViewModel
                {
                    documentationId = cam.LOANREVIEWCAMID,
                    documentation = cam.DOCUMENTATION,
                    approvalLevelId = cam.APPROVALLEVELID,
                    applicationId = cam.LOANREVIEWAPPLICATIONID,
                    referenceNumber = cam.CAMREF,
                }).ToList();
        }

        public CamViewModel GetCamDocumentByApprovalLevel(int applicationId, int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CAM).ToList();

            var cams = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Where(x =>
                x.LOANREVIEWAPPLICATIONID == applicationId
            //&& x.APPROVALLEVELID == approvalLevelId
            );

            if (cams.Any() == false) return new CamViewModel();

            TBL_LOAN_REVIEW_APPLICATN_CAM cam;

            if (cams.Any(x => ids.Contains(x.APPROVALLEVELID)) == true)
                cam = cams.Where(x => ids.Contains(x.APPROVALLEVELID)).OrderByDescending(x => x.LOANREVIEWCAMID).FirstOrDefault();
            else
                cam = cams.OrderByDescending(x => x.LOANREVIEWCAMID).FirstOrDefault();

            return new CamViewModel
            {
                documentationId = cam.LOANREVIEWCAMID,
                documentation = cam.DOCUMENTATION,
                approvalLevelId = cam.APPROVALLEVELID,
                applicationId = cam.LOANREVIEWAPPLICATIONID,
                referenceNumber = cam.CAMREF,
            };
        }

        public CamViewModel GetCamDocument(int documentationId)
        {
            var cam = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Find(documentationId);
            if (cam == null) return new CamViewModel();

            return new CamViewModel
            {
                documentationId = cam.LOANREVIEWCAMID,
                documentation = cam.DOCUMENTATION,
                approvalLevelId = cam.APPROVALLEVELID,
                applicationId = cam.LOANREVIEWAPPLICATIONID,
                referenceNumber = cam.CAMREF,
            };
        }

        public int ForwardApplication(ForwardReviewViewModel model)
        {
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);

            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal)
            {
                if (appl.CUSTOMERID > 0) workflow.Amount = GetCustomerTotalOutstandingBalance((int)appl.CUSTOMERID);
            }

            workflow.StaffId = model.lastUpdatedBy;
            workflow.CompanyId = model.companyId;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.ProductClassId = null;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            context.SaveChanges();          

            int lastOperationId = (int)OperationsEnum.LoanReviewApprovalAvailment;

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                int lastStatusId = workflow.StatusId;
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved && model.operationId != lastOperationId) // jump process OR end flag
                {
                    workflow.NextProcess(model.companyId, model.lastUpdatedBy, model.operationId + 1, model.applicationId, null, "New application", true, true);
                }
                appl.APPROVALSTATUSID = (short)lastStatusId;
                context.SaveChanges();
                return lastStatusId;
            }

            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal)
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                context.SaveChanges();
            }

            return (int)ApprovalStatusEnum.Processing; // default for now
        }

        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            decimal loanBalance = 0;
            decimal overdraftBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN
                               where a.CUSTOMERID == customerId
                               select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }
            else
            {
                loanBalance = 0;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING
                               where a.CUSTOMERID == customerId
                               select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }
            else
            {
                overdraftBalance = 0;
            }

            decimal totalBalance = loanBalance + overdraftBalance;

            return totalBalance;
        }

    }
}
