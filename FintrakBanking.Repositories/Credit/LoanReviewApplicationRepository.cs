using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private IAdminRepository admin;

        private CreditCommonRepository creditCommon;

        private List<int> camOperationIds = new List<int> { 46, 71, 79 }; // RMU(71), CAM(79)

        private readonly int classifiedAssetManagementRoleId = 46;

        public LoanReviewApplicationRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository audit,
            IWorkflow workflow,
            IAdminRepository admin,

        CreditCommonRepository creditCommon
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.admin = admin;
            this.creditCommon = creditCommon;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;

            bool ignoreBranch = true; // rm = false, ho = true
            if (operationId == 47) if (ProcessInitiator(staffId, operationId, classId, 2)) ignoreBranch = false;
            if (camOperationIds.Contains(operationId)) if (ProcessInitiator(staffId, operationId, classId, 1)) ignoreBranch = false;

            List<int> operationIds = new List<int>();
            operationIds.Add(operationId);
            if (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) operationIds.Add((int)OperationsEnum.NPLoanReviewApprovalAppraisal);
            if (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) operationIds.Add(79);

            IQueryable<LoanReviewApplicationViewModel> applications = null;

            // get approval levels 
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            var ids = levelIds.ToList();
            ids.Add(71); // --------------- REMOVE!!!
            ids.Add(79); // --------------- REMOVE!!!

            // query
            var query = context.TBL_LMSR_APPLICATION.Where(x => x.BRANCHID == user.BranchId || ignoreBranch)
            .Join(context.TBL_BRANCH, a => a.BRANCHID, b => b.BRANCHID, (a, b) => new { a, b })
            .Join(context.TBL_CUSTOMER, ab => ab.a.CUSTOMERID, c => c.CUSTOMERID, (ab, c) => new { ab, c, b = ab.b })
            .Join(context.TBL_APPROVAL_TRAIL.Where(x => operationIds.Contains(x.OPERATIONID)
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && x.RESPONSESTAFFID == null
                    && (x.TBL_APPROVAL_LEVEL1.LEVELTYPEID != 2 || operationIds.Contains(48))
                    && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                    && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)
            ),
                alaba => alaba.ab.a.LOANAPPLICATIONID,
                trail => trail.TARGETID,
                (alaba, trail) => new { application = alaba.ab.a, trail, branch = alaba.b, customer = alaba.c })
            .Select(x => new LoanReviewApplicationViewModel
            {
                //approvalStateId = trail == null ? 0 : trail.APPROVALSTATEID,
                approvalState = x.trail == null ? "Pending" : x.trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalTrailId = x.trail == null ? 0 : x.trail.APPROVALTRAILID,
                currentApprovalLevel = x.trail == null ? "" : x.trail.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                // currentApprovalLevelTypeId = x.trail == null ? null : x.trail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                currentApprovalLevelId = x.trail == null ? 0 : x.trail.TOAPPROVALLEVELID,
                lastComment = x.trail == null ? "" : x.trail.COMMENT,
                toStaffId = x.trail == null ? 0 : x.trail.TOSTAFFID,
                requestStaffId = x.trail == null ? 0 : x.trail.REQUESTSTAFFID,

                applicationDate = x.application.APPLICATIONDATE,
                approvalStatus = x.application.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                approvalStatusId = x.application.APPROVALSTATUSID,
                createdBy = x.application.CREATEDBY,
                loanReviewApplicationId = x.application.LOANAPPLICATIONID,
                referenceNumber = x.application.APPLICATIONREFERENCENUMBER,
                branchId = x.branch.BRANCHID,
                branchName = x.branch.BRANCHNAME,
                customerId = x.customer.CUSTOMERID,
                operationId = x.application.OPERATIONID,
                customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,
                atInitiator = x.application.CREATEDBY == staffId,

                // currentStage = trail == null ? "" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == trail.OPERATIONID).OPERATIONNAME,

                applicationDetails = x.application.TBL_LMSR_APPLICATION_DETAIL.Select(d => new applicationDetails
                {
                    detailId = d.LOANREVIEWAPPLICATIONID,
                    operationId = d.OPERATIONID,
                    operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                    reviewDetails = d.REVIEWDETAILS,
                    loanId = d.LOANID,
                    loanSystemTypeId = d.LOANSYSTEMTYPEID,
                    loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                    productId = d.PRODUCTID,
                    customerId = d.CUSTOMERID,
                    obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                    proposedTenor = d.PROPOSEDTENOR,
                    proposedRate = d.PROPOSEDINTERESTRATE,
                    proposedAmount = d.PROPOSEDAMOUNT,
                    approvedTenor = d.APPROVEDTENOR,
                    approvedRate = d.APPROVEDINTERESTRATE,
                    approvedAmount = d.APPROVEDAMOUNT,
                    customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                    //loanReferenceNumber = d.LOANREFERENCENUMBER,

                })

            })
            .GroupBy(d => d.loanReviewApplicationId)
            .ToList()
            ;

            applications = query.AsQueryable()           
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.loanReviewApplicationId);

            var list = applications.ToList();
            //var count = applications.Count();

            return applications;//.Where(x => x.currentApprovalLevelTypeId != 2 || operationIds.Contains(48)); // .Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        private bool ProcessInitiator(int staffId, int operationId, int? productClassId, int position)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);

            return index == (position - 1);
        }

        public List<LMSOperationListViewModel> GetApplicationOperations()
        {

            var operations = (from a in context.TBL_OPERATIONS
                              select a).ToList();

            List<LMSOperationListViewModel> output = new List<LMSOperationListViewModel>();

            //LMSOperationListViewModel value = new LMSOperationListViewModel {operationId = (short)OperationsEnum.TenorChange, operationName = "Tenor Change", loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability };

            // ----------------contingent liability --------------------------------------
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.ContingentLiabilityRenewal,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.ContingentLiabilityRenewal).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability,
                productTypeId = (short)LoanProductTypeEnum.ContingentLiability
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.ContingentLiabilityTermination,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.ContingentLiabilityTermination).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.ContingentLiability,
                productTypeId = (short)LoanProductTypeEnum.ContingentLiability
            });

            //---------------------overdraft/revolving------------------------------------
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftInterestRate,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftInterestRate).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftRenewal,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftRenewal).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftSubAllocation,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftSubAllocation).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftTenorExtension,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftTenorExtension).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });
            output.Add(new LMSOperationListViewModel
            {
                operationId = (short)OperationsEnum.OverdraftTopup,
                operationName = operations.FirstOrDefault(x => x.OPERATIONID == (short)OperationsEnum.OverdraftTopup).OPERATIONNAME,
                loanSystemTypeId = (short)LoanSystemTypeEnum.OverdraftFacility,
                productTypeId = (short)LoanProductTypeEnum.RevolvingLoan
            });




            return output;
        }

        public SelectListViewModel GetAllSelectList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x =>
                (x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement
                || x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft
                || x.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial)
                && x.ISDISABLED == false
            ).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME, typeId = (int)x.OPERATIONTYPEID }).OrderBy(o => o.name).ToList();
            list.feeCharges = context.TBL_CHARGE_FEE.Select(x => new DropDownSelect { id = x.CHARGEFEEID, name = x.CHARGEFEENAME }).ToList();
            return list;
        }

        public string SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            int staffId = model.createdBy;
            var referenceNumber = GenerateReferenceNumber();
            var applicationDate = general.GetApplicationDate();
            int camOperationId = GetCamOperation(model.performanceTypeId);
            bool result = true;


            foreach (var detail in model.applicationDetails)
            {

                if (detail.operationId == (int)OperationsEnum.CommercialLoanSubAllocation)
                {
                    result = ValidateNewSubAllocationOperation(detail.detailId, model.customerId, detail.loanSystemTypeId);

                    if (result == false)
                        throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");

                }
                else if (detail.operationId == (int)OperationsEnum.OverdraftSubAllocation)
                {
                    result = ValidateNewSubAllocationOperation(detail.detailId, model.customerId, detail.loanSystemTypeId);

                    if (result == false)
                        throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");

                }
                //else if(detail.operationId == (int)OperationsEnum.)
                //{

                //}

            }

            var application = context.TBL_LMSR_APPLICATION.Add(new TBL_LMSR_APPLICATION
            {
                APPLICATIONREFERENCENUMBER = referenceNumber,
                COMPANYID = model.companyId,
                CUSTOMERID = model.customerId,
                BRANCHID = model.branchId,
                OPERATIONID = camOperationId,
                CAPREGIONID = model.regionId,
                // CUSTOMERGROUPID = null,
                DISPUTED = false,
                REQUIRECOLLATERAL = false,
                APPLICATIONDATE = applicationDate,
                CREATEDBY = staffId,
                DATETIMECREATED = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONSTATUSID = (short)1, // -------------------------------------------------- REMOVE COLUMN!!
            });

            List<int> customerIds = new List<int>();
            LoanViewModel loan = new LoanViewModel();

            foreach (var detail in model.applicationDetails)
            {
                loan = GetLoanInformation(detail.loanSystemTypeId, detail.loanId, applicationDate);
                int tenor = detail.loanSystemTypeId == 4 ? loan.tenorUsed : loan.tenor;

                context.TBL_LMSR_APPLICATION_DETAIL.Add(new TBL_LMSR_APPLICATION_DETAIL
                {
                    LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                    LOANID = detail.loanId,
                    LOANSYSTEMTYPEID = detail.loanSystemTypeId,/*Term/Disbursed Facility..Overdraft Facility..Contingent Liability*/
                    OPERATIONID = detail.operationId, // refactor to operationId from ui!
                    REVIEWDETAILS = detail.reviewDetails,
                    PRODUCTID = detail.productId,
                    REPAYMENTTERMS = String.Empty,
                    REPAYMENTSCHEDULE = String.Empty,
                    CUSTOMERID = loan.customerId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved, // REMOVE DUPLICATE [STATUSID]
                    CREATEDBY = staffId,
                    DATETIMECREATED = applicationDate,
                    PROPOSEDTENOR = tenor,
                    PROPOSEDINTERESTRATE = loan.interestRate,
                    PROPOSEDAMOUNT = loan.outstandingPrincipal,
                    APPROVEDTENOR = tenor,
                    APPROVEDINTERESTRATE = loan.interestRate,
                    APPROVEDAMOUNT = loan.outstandingPrincipal,
                    OPERATIONPERFORMED = false,
                    CUSTOMERPROPOSEDAMOUNT = detail.customerProposedAmount,
                    //LOANREFERENCENUMBER = loan.loanReferenceNumber

                    //LOANAPPLICATIONDETAILID = loan.loanApplicationDetailId,
                });

                customerIds.Add(loan.customerId);
            }

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) throw new SecureException("An error occured while saving the data!"); // this save is necessary to grab targetid

            workflow.ToStaffId = staffId;

            bool assetManagement = false;
            var user = context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == staffId);
            if (user.STAFFROLEID == classifiedAssetManagementRoleId) assetManagement = true;

            if (assetManagement)
            {
                workflow.NextProcess(model.companyId, staffId, 79, application.LOANAPPLICATIONID, null, "NIL", true, true, true);
                application.OPERATIONID = 79;
                context.Entry(application).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                workflow.NextProcess(model.companyId, staffId, camOperationId, application.LOANAPPLICATIONID, null, "NIL", true, true, true);
            }

            if (context.SaveChanges() > 0)
            {
                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION) creditCommon.LoadCustomerTurnover(application.LOANAPPLICATIONID,customerIds,staffId, true);

                return "Application with reference number " + referenceNumber + " created.";
            }

            throw new SecureException("An error occured while saving the data!");
        }

        public bool ValidateSubAllocationOperation(int loanApplicationDetailId, int customerId)
        {

            if (loanApplicationDetailId != 0)
            {
                var loanData = (from a in context.TBL_LOAN
                                join b in context.TBL_PRODUCT
                                on a.PRODUCTID equals b.PRODUCTID
                                where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                && b.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                if (loanData.Count < 2 || loanData == null)
                {
                    return false; // throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");  
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var appplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId).Select(x => x.LOANAPPLICATIONID).FirstOrDefault();

                var loanData = (from a in context.TBL_LOAN_REVOLVING
                                where a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == appplicationId // .CUSTOMERID == customerId
                                && a.MATURITYDATE < context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                //var test = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

                if (loanData.Count < 2 || loanData == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }



        }

        public bool ValidateNewSubAllocationOperation(int loanApplicationDetailId, int customerId, int loanSystemTypeId)
        {

            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                var loanData = (from a in context.TBL_LOAN
                                join b in context.TBL_PRODUCT
                                on a.PRODUCTID equals b.PRODUCTID
                                where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                && b.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                if (loanData.Count < 2 || loanData == null)
                {
                    return false; // throw new ConditionNotMetException("Customer Must Have More Than One Tranch to Proceed With Sub Allocation");  
                }
                else
                {
                    return true;
                }
            }
            else
            {
                var appplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId).Select(x => x.LOANAPPLICATIONID).FirstOrDefault();

                var loanData = (from a in context.TBL_LOAN_REVOLVING
                                join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                where b.LOANAPPLICATIONID == appplicationId
                                && a.MATURITYDATE >= context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE
                                && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                select a).ToList();
                //var test = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

                if (loanData.Count < 2 || loanData == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }



        }
               
        private int GetCamOperation(int performanceTypeId)
        {
            switch (performanceTypeId)
            {
                case 2: return 71;
                case 3: return 79; // ======== recovery ========
            }
            return 46;
        }

        private string GenerateReferenceNumber()
        {
            int length = 10;
            string input = "0000000001";
            string output = "0000000001";
            var appl = context.TBL_LMSR_APPLICATION.OrderByDescending(x => x.LOANAPPLICATIONID).FirstOrDefault();
            if (appl != null) input = appl.APPLICATIONREFERENCENUMBER;
            output = (int.Parse(input) + 1).ToString().PadLeft(length, '0');
            return output;
        }

        public WorkflowResponse ForwardApplication(ForwardReviewViewModel model)
        {
            int nextProcessId = model.operationId + 1;
            int operationId = model.operationId; // beware of nplappraisal!
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);
            int lastOperationId = (int)OperationsEnum.LoanReviewApprovalAvailment;

            var checklistValidation = ChecklistCompleted(model.applicationId);
            if (appl.CREATEDBY == model.createdBy  && model.operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter && checklistValidation == false)
            {
                throw new SecureException("Checklist not complleted!");
            }

            // customization for CAM approvals
            //bool operationIsCam = (operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal) || (operationId == (int)OperationsEnum.NPLoanReviewApprovalAppraisal);
            if (camOperationIds.Contains(operationId))
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                operationId = (int)appl.OPERATIONID;
                nextProcessId = (int)OperationsEnum.LoanReviewApprovalOfferLetter; // redefine
                if (appl.CUSTOMERID > 0) workflow.Amount = GetCustomerTotalOutstandingBalance((int)appl.CUSTOMERID);
            }

            workflow.StaffId = model.lastUpdatedBy;
            workflow.CompanyId = appl.COMPANYID;
            workflow.OperationId = operationId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.ProductClassId = null;
            workflow.StatusId = model.forwardAction;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;
            workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;

            if (model.forwardAction == 11 || model.forwardAction == 12)
            {
                workflow.StatusId = (int)ApprovalStatusEnum.Referred;
                var dictionary = GetRepresentStepdownItems(appl.LOANAPPLICATIONID, model.forwardAction,operationId);
                workflow.NextLevelId = dictionary["levelId"];
                workflow.ToStaffId = dictionary["staffId"];
            }

            workflow.LogActivity();

            context.SaveChanges(); // redundant !

            // DETAIL CHANGES
            List<TBL_LMSR_APPLICATION_DETAIL> items = null;
            if (model.recommendedChanges != null && model.recommendedChanges.Count() > 0) // only approving authority
            {
                //updateApprovedAmount = true;
                items = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID).ToList();
                foreach (var changed in model.recommendedChanges)
                {
                    var detail = items.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == changed.detailId);
                    if (detail != null)
                    {
                        //detail.APPROVEDPRODUCTID = (short)changed.productId;
                        detail.APPROVEDAMOUNT = changed.amount;
                        detail.APPROVEDINTERESTRATE = changed.interestRate;
                        detail.APPROVEDTENOR = changed.tenor;
                    }
                }
            }
            vv
            int lastStatusId = workflow.StatusId;
            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved && operationId != lastOperationId/* && model.operationId != 71*/) // jump process OR end flag
                {
                    if (operationId == (int)OperationsEnum.LoanReviewApprovalOfferLetter) workflow.NextLevelId = GetFirstReceiverLevel(model.lastUpdatedBy, (int)OperationsEnum.LoanReviewApprovalAvailment, null, true);
                    workflow.NextProcess(appl.COMPANYID, model.lastUpdatedBy, nextProcessId, appl.LOANAPPLICATIONID, null, "New application", true, true); // model.operationId must be used here!
                }

                if (operationId == lastOperationId/* || model.operationId == 71*/) appl.APPROVALSTATUSID = (short)lastStatusId; // last or cam?

                context.SaveChanges();

                AddLoanCollateralMapping(model.applicationId);//, appl., (short)LoanSystemTypeEnum.OverdraftFacility);
            }

            //return lastStatusId;
            return workflow.Response;
        }

        private int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var contingentData = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CUSTOMERID == customerId);

            decimal loanBalance = 0;
            decimal overdraftBalance = 0;
            decimal contingentBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN where a.CUSTOMERID == customerId select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING where a.CUSTOMERID == customerId select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }

            if (contingentData != null)
            {
                var balance = (from a in context.TBL_LOAN_CONTINGENT where a.CUSTOMERID == customerId select a.CONTINGENTAMOUNT).Sum();
                contingentBalance = balance;
            }

            decimal totalBalance = loanBalance + overdraftBalance + contingentBalance;

            return totalBalance;
        }

        public LoanViewModel GetLoanInformation(int loanSystemTypeId, int loanId, DateTime startDate)
        {
            var result = new LoanViewModel();
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                result = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OUTSTANDINGPRINCIPAL,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = loan.INTERESTRATE,
                    outstandingPrincipal = loan.OVERDRAFTLIMIT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    maturityDate = loan.MATURITYDATE,
                    interestRate = 0,
                    outstandingPrincipal = loan.CONTINGENTAMOUNT,
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.LOANREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.LineFacility)
            {
                result = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == loanId).Select(loan => new LoanViewModel
                {
                    customerId = loan.CUSTOMERID,
                    effectiveDate = startDate,
                    tenorUsed = loan.APPROVEDTENOR,
                    interestRate = loan.APPROVEDINTERESTRATE,
                    outstandingPrincipal = loan.APPROVEDAMOUNT, // adapting!
                    loanApplicationDetailId = loan.LOANAPPLICATIONDETAILID,
                    loanReferenceNumber = loan.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                })
                .FirstOrDefault();
            }
            else
            {
                throw new SecureException("The Product type is Invalid");
            }
            return result;
        }

        public LoanApplicationDetailViewModel GetLoanApplicationDetail(int loanId, int loanTypeId)
        {
            int id = GetLoanApplicationDetailId(loanId, loanTypeId);

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == id);
            if (detail == null) throw new Exception("Could not find loan application detail with id of " + id);

            return new LoanApplicationDetailViewModel
            {
                loanApplicationDetailId = detail.LOANAPPLICATIONDETAILID,
                loanApplicationId = detail.LOANAPPLICATIONID,
            };
        }

        private int GetLoanApplicationDetailId(int loanId, int loanTypeId)
        {
            int id = 0;
            if (loanTypeId == 1)
            {
                var loan = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 2)
            {
                var loan = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 3)
            {
                var loan = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == loanId);
                id = loan.LOANAPPLICATIONDETAILID;
            }
            if (loanTypeId == 4)
            {
                id = loanId;
            }
            return id;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetRegionalLoanApplications(int staffId)
        {
            List<int> levels1 = general.GetRouteLevels(46, 1);
            List<int> levels2 = general.GetRouteLevels(71, 1);
            List<int> levels3 = general.GetRouteLevels(79, 1);

            var levels = levels1.Union(levels2).Union(levels3).Distinct();

            //var branches = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
            //                    .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
            //                    .Join(context.TBL_BRANCH, sr => sr.r.REGIONID, b => b.REGIONID, (sr, b) => new { sr, b })
            //                    .Select(x => new {
            //                        BRANCHID = x.b.BRANCHID
            //                    })
            //                    .Select(x => x.BRANCHID)
            //                    .ToList();

            var regions = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
                            .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
                            .Select(x => new
                            {
                                REGIONID = x.r.REGIONID
                            })
                            .Select(x => x.REGIONID)
                            .ToList();

            var applications = context.TBL_LMSR_APPLICATION.Where(x =>
                    regions.Contains((int)x.CAPREGIONID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                )
                .OrderByDescending(x => x.LOANAPPLICATIONID)
                .Join(
                    context.TBL_APPROVAL_TRAIL.Where(x => camOperationIds.Contains(x.OPERATIONID)
                        && levels.Contains((int)x.TOAPPROVALLEVELID)
                        && (x.RESPONSESTAFFID == null || (x.RESPONSESTAFFID == staffId && x.TOSTAFFID != null))
                        ),// && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)),

                    a => a.LOANAPPLICATIONID,
                    b => b.TARGETID,
                    (a, b) => new { a, b, branch = a.TBL_BRANCH, customer = a.TBL_CUSTOMER })
                .Select(x => new LoanReviewApplicationViewModel
                {
                    approvalState = x.b.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalTrailId = x.b.APPROVALTRAILID,
                    currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                    lastComment = x.b.COMMENT,
                    toStaffId = x.b.TOSTAFFID,

                    applicationDate = x.a.APPLICATIONDATE,
                    approvalStatus = x.a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    approvalStatusId = x.a.APPROVALSTATUSID,
                    createdBy = x.a.CREATEDBY,
                    loanReviewApplicationId = x.a.LOANAPPLICATIONID,
                    referenceNumber = x.a.APPLICATIONREFERENCENUMBER,

                    branchId = x.branch.BRANCHID,
                    branchName = x.branch.BRANCHNAME,
                    customerId = x.customer.CUSTOMERID,
                    operationId = x.a.OPERATIONID,
                    customerName = x.customer.FIRSTNAME + " " + x.customer.MIDDLENAME + " " + x.customer.LASTNAME,

                    timeIn = x.b.SYSTEMARRIVALDATETIME,
                    timeOut = x.b.SYSTEMRESPONSEDATETIME,
                    responsiblePerson = context.TBL_STAFF
                                            .Where(s => s.STAFFID == x.b.TOSTAFFID)
                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                            .FirstOrDefault().name ?? "",
                    requestStaffId = x.b.REQUESTSTAFFID,
                    toApprovalLevelId = x.b.TOAPPROVALLEVELID,

                    applicationDetails = x.a.TBL_LMSR_APPLICATION_DETAIL.Select(d => new applicationDetails
                    {
                        detailId = d.LOANREVIEWAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        loanId = d.LOANID,
                        loanSystemTypeId = d.LOANSYSTEMTYPEID,
                        loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                        productId = d.PRODUCTID,
                        customerId = d.CUSTOMERID,
                        obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                        proposedTenor = d.PROPOSEDTENOR,
                        proposedRate = d.PROPOSEDINTERESTRATE,
                        proposedAmount = d.PROPOSEDAMOUNT,
                        approvedTenor = d.APPROVEDTENOR,
                        approvedRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                    })
                })
                .GroupBy(d => d.approvalTrailId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                ;

            return applications;

        }

        #region
        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            searchString = searchString.Trim().ToLower();

            int[] operations = { (int)OperationsEnum.LoanReviewApprovalAppraisal, (int)OperationsEnum.LoanReviewApprovalOfferLetter, (int)OperationsEnum.LoanReviewApprovalAvailment ,
           (int)OperationsEnum.NPLoanReviewApprovalAppraisal,(int)OperationsEnum.WrittenOffLoanReviewApprovalAppraisal};

            var applications = from a in context.TBL_LMSR_APPLICATION
                               join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                               join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
                               join y in context.TBL_APPROVAL_TRAIL on d.LOANREVIEWAPPLICATIONID equals y.TARGETID
                               let staffcode = context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault()
                               where (a.APPLICATIONREFERENCENUMBER.ToLower().Contains(searchString)
                               || g.FIRSTNAME.ToLower().Contains(searchString)
                               || g.LASTNAME.ToLower().Contains(searchString)
                               || g.MIDDLENAME.ToLower().Contains(searchString)
                               || d.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower().Contains(searchString)).Select(o => o.STAFFID).FirstOrDefault()
                               )
                               select new LoanApplicationViewModel
                               {
                                   firstName = g.FIRSTNAME,
                                   middleName = g.MIDDLENAME,
                                   lastName = g.LASTNAME,
                                   customerCode = g.CUSTOMERCODE,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   loanApplicationId = a.LOANAPPLICATIONID,
                                   customerId = a.CUSTOMERID,
                                   branchId = a.BRANCHID,
                                   customerGroupId = a.CUSTOMERGROUPID,
                                   applicationDate = a.APPLICATIONDATE,
                                   applicationAmount = d.PROPOSEDAMOUNT,
                                   approvedAmount = d.APPROVEDAMOUNT,
                                   interestRate = d.PROPOSEDINTERESTRATE,
                                   applicationTenor = d.PROPOSEDTENOR,
                                   approvalStatusId = (short)a.APPROVALSTATUSID,
                                   approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                   currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                   approvalTrailId = y.APPROVALTRAILID,
                                   responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,
                                   applicationStatusId = a.APPLICATIONSTATUSID,
                                   applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                   branchName = a.TBL_BRANCH.BRANCHNAME,
                                   relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                   createdBy = a.CREATEDBY,
                                   operationId = a.OPERATIONID,
                                   isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
                               };

            //var applications = from a in context.TBL_LMSR_APPLICATION
            //                   join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
            //                   join g in context.TBL_CUSTOMER on d.CUSTOMERID equals g.CUSTOMERID
            //                   join y in context.TBL_APPROVAL_TRAIL on d.LOANREVIEWAPPLICATIONID equals y.TARGETID
            //                   let staffcode = context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault()
            //                   where (a.APPLICATIONREFERENCENUMBER == searchString
            //                   || g.FIRSTNAME.ToLower().StartsWith(searchString)
            //                   || g.LASTNAME.ToLower().StartsWith(searchString)
            //                   || g.MIDDLENAME.ToLower().StartsWith(searchString)
            //                   || d.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
            //                   select new LoanApplicationViewModel
            //                   {
            //                       firstName = g.FIRSTNAME,
            //                       middleName = g.MIDDLENAME,
            //                       lastName = g.LASTNAME,
            //                       customerCode = g.CUSTOMERCODE,
            //                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
            //                       loanApplicationId = a.LOANAPPLICATIONID,
            //                       customerId = a.CUSTOMERID,
            //                       branchId = a.BRANCHID,
            //                       customerGroupId = a.CUSTOMERGROUPID,
            //                       applicationDate = a.APPLICATIONDATE,
            //                       applicationAmount = d.PROPOSEDAMOUNT,
            //                       approvedAmount = d.APPROVEDAMOUNT,
            //                       interestRate = d.PROPOSEDINTERESTRATE,
            //                       applicationTenor = d.PROPOSEDTENOR,
            //                       approvalStatusId = (short)a.APPROVALSTATUSID,
            //                       approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
            //                       currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
            //                       approvalTrailId = y.APPROVALTRAILID,
            //                       responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,
            //                       applicationStatusId = a.APPLICATIONSTATUSID,
            //                       applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(k => k.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).Select(k => k.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
            //                       branchName = a.TBL_BRANCH.BRANCHNAME,
            //                       relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
            //                       relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_LOAN.Where(k => k.TERMLOANID == d.LOANID).Select(k => k.RELATIONSHIPOFFICERID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
            //                       createdBy = a.CREATEDBY,
            //                       operationId = a.OPERATIONID,
            //                       isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == a.APPLICATIONREFERENCENUMBER).Any()
            //                   };

            return applications;
        }

        private IQueryable<LoanApplicationViewModel> GetLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationViewModel
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID ?? 0,
                            customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            misCode = a.MISCODE,
                            teamMisCode = a.TEAMMISCODE,
                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                            customerGroupId = a.CUSTOMERGROUPID ?? 0,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            createdBy = a.CREATEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            applicationTenor = a.APPLICATIONTENOR,
                            applicationAmount = a.APPLICATIONAMOUNT,
                            dateTimeCreated = a.DATETIMECREATED,
                            LoanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                             .Select(c => new LoanApplicationDetailViewModel()
                             {
                                 equityAmount = c.EQUITYAMOUNT,
                                 equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                 approvedAmount = c.APPROVEDAMOUNT,
                                 approvedInterestRate = c.APPROVEDINTERESTRATE,
                                 approvedProductId = c.APPROVEDPRODUCTID,
                                 approvedTenor = c.APPROVEDTENOR,
                                 currencyId = c.CURRENCYID,
                                 currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                 customerId = c.CUSTOMERID,
                                 exchangeRate = c.EXCHANGERATE,
                                 loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                 subSectorId = c.SUBSECTORID,
                                 loanApplicationId = c.LOANAPPLICATIONID,
                                 proposedAmount = c.PROPOSEDAMOUNT,
                                 proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                 proposedProductId = c.PROPOSEDPRODUCTID,
                                 proposedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                 //proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                                 statusId = c.STATUSID
                             }).ToList()
                        });
            return data;
        }


        #endregion

        public bool AppraisalReviewReferBack(ForwardViewModel model)
        {
            var o = context.TBL_APPROVAL_TRAIL.Find(model.trailId); // here we try to get the staffid on the trail row
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);

            var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == model.operationId
                && x.TARGETID == appl.LOANAPPLICATIONID
                && x.REQUESTSTAFFID == o.REQUESTSTAFFID
            );

            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = model.productClassId;//.PRODUCTCLASSID;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = trail.FROMAPPROVALLEVELID;//
            workflow.ToStaffId = o.REQUESTSTAFFID;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            // Take out of offer letter screen
            var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == (int)OperationsEnum.LoanReviewApprovalOfferLetter
                && x.RESPONSESTAFFID == null
                && x.TARGETID == appl.LOANAPPLICATIONID
            );
            if (currentTrail != null)
            {
                currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                currentTrail.COMMENT = model.comment;
                currentTrail.TOAPPROVALLEVELID = null;
                currentTrail.TOSTAFFID = null;
            }
            appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
            appl.OPERATIONID = model.operationId;

            return context.SaveChanges() > 0;
        }

        public bool UpdateManagementPosition(ManagementPositionViewModel model)
        {
            var entity = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == model.applicationDetailId);
            if (entity == null) return false;
            entity.MANAGEMENTPOSITION = model.managementPosition;
            context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public ManagementPositionViewModel GetManagementPosition(int detailId)
        {
            var entity = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == detailId);

            ManagementPositionViewModel position = new ManagementPositionViewModel();
            if (entity == null) return position;
            position.managementPosition = entity.MANAGEMENTPOSITION;
            position.applicationDetailId = detailId;
            return position;
        }

        public bool ChecklistCompleted(int applicationId)
        {
            var condition = (from c in context.TBL_LMSR_CONDITION_PRECEDENT
                             where c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId
                             && c.ISEXTERNAL == true && c.ISSUBSEQUENT == false
                             select c).ToList();

            var status = (from c in context.TBL_LMSR_CONDITION_PRECEDENT
                          where c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId
                          && c.ISEXTERNAL == true && c.ISSUBSEQUENT == false && c.CHECKLISTSTATUSID != null
                          select c).ToList();

            return condition.Count == status.Count;
        }

        public List<LoanReviewOperationViewModel> GetMaturityInstruction(int loanId, short loansystemTypeId)
        {
            var ops = (from op in context.TBL_LOAN_MATURITY_INSTRUCTION
                       where op.LOANID == loanId && op.LOANSYSTEMTYPEID == loansystemTypeId
                       select new LoanReviewOperationViewModel
                       {
                           isUsed = op.ISUSED == false ? "False" : "True",
                           tenor = op.TENOR,
                           approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == op.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           instructionType = context.TBL_LOAN_MATURITY_INSTRU_TYPE.Where(x=> x.INSTRUCTIONTYPEID == op.INSTRUCTIONTYPEID).Select(x=> x.INSTRUCTIONTYPENAME).FirstOrDefault(),
                           actionBy = context.TBL_STAFF.Where(o=> o.STAFFID == op.CREATEDBY).Select( x=> new { Name = x.LASTNAME + ", " + x.FIRSTNAME }).FirstOrDefault().Name,
                       });

            return ops.ToList();
        }

        public List<LoanReviewOperationViewModel> GetLMSOperation(int loanId, short loansystemTypeId)
        {
            var ops = (from op in context.TBL_LOAN_REVIEW_OPERATION
                       where op.LOANID == loanId && op.LOANSYSTEMTYPEID == loansystemTypeId
                       select new LoanReviewOperationViewModel
                       {
                           operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == op.OPERATIONTYPEID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                           reviewDetails = op.REVIEWDETAILS,
                           proposedEffectiveDate = op.EFFECTIVEDATE,
                           approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == op.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           operationCompleted = op.OPERATIONCOMPLETED,
                           loanApplicationId = 0,
                           loanReviewOperationsId = op.LOANREVIEWOPERATIONID
                       });

            return ops.ToList();
        }

        public bool AddLoanCollateralMapping(int loanApplicationId)
        {
            LoanApplicationViewModel appl;
            List<int> existingCollateralIds;
            List<TBL_LOAN_APPLICATION_COLLATERL> recommendedCollaterals;

            var details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
            
            foreach(var d in details)
            {
                appl = GetLoanApplicationByLoanSystemType(d.LOANSYSTEMTYPEID, d.LOANID);

                existingCollateralIds = context.TBL_LOAN_COLLATERAL_MAPPING
                    .Where(x => x.LOANID == d.LOANID && x.ISRELEASED == false)
                    .Select(x => x.COLLATERALCUSTOMERID)
                    .ToList();

                recommendedCollaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == appl.loanApplicationId).ToList();

                foreach (var recommended in recommendedCollaterals)
                {
                    if (existingCollateralIds.Contains(recommended.COLLATERALCUSTOMERID)) continue;
                    context.TBL_LOAN_COLLATERAL_MAPPING.Add(new TBL_LOAN_COLLATERAL_MAPPING
                    {
                        COLLATERALCUSTOMERID = recommended.COLLATERALCUSTOMERID,
                        LOANID = d.LOANID,
                        LOANSYSTEMTYPEID = d.LOANSYSTEMTYPEID,
                        ISRELEASED = false,
                    });
                }
            }

            return context.SaveChanges() > 0;
        }

        private LoanApplicationViewModel GetLoanApplicationByLoanSystemType(int loanSystemTypeId, int loanId)
        {
            var result = new LoanApplicationViewModel();

            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                result = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, l => l.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (l, d) => new { l, d })
                    .Select(x => new LoanApplicationViewModel { loanApplicationId = x.d.LOANAPPLICATIONID })
                    .FirstOrDefault();
            }
            else
            {
                throw new SecureException("Collateral Failed To Map. Loan System Type could not be resolved!");
            }

            if (result.loanApplicationId < 1) throw new SecureException("Collateral Failed To Map. Error resolving Loan Application Information.");

            return result;
        }

        private Dictionary<string, int> GetRepresentStepdownItems(int applicationId, int action, int operationId)
        {
            int levelId;
            int staffId;

            var trails = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && x.TARGETID == applicationId
                    && x.FROMAPPROVALLEVELID != null
                    && x.TOAPPROVALLEVELID != null
                ).OrderBy(x => x.APPROVALTRAILID);

            if (action == 11)
            {
                var traill = trails.Join(context.TBL_APPROVAL_LEVEL.Where(x => x.LEVELTYPEID == 2)
                        , t => t.FROMAPPROVALLEVELID, l => l.APPROVALLEVELID, (t, l) => new { t, l })
                        .Select(x => new { x.t }).First();
                staffId = traill.t.REQUESTSTAFFID;
                levelId = (int)traill.t.FROMAPPROVALLEVELID;
            }
            else
            {
                var trail = trails.FirstOrDefault();
                staffId = trail.REQUESTSTAFFID;
                levelId = (int)trail.FROMAPPROVALLEVELID;
            }

            if (levelId < 1 || staffId < 1) throw new SecureException("Error while resolving receiving staff.");

            var data = new Dictionary<string, int>();
            data.Add("levelId", levelId);
            data.Add("staffId", staffId);

            return data;
        }
    }
}
