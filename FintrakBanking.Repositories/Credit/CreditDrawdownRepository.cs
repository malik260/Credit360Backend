using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Common.CustomException;
using FinTrakBanking.ThirdPartyIntegration;
using FintrakBanking.ViewModels.Flexcube;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace FintrakBanking.Repositories.Credit
{
    public class CreditDrawdownRepository : ICreditDrawdownRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;
        //private IAuditTrailRepository audit;
        private IOverRideRepository overrider;
        private IntegrationWithFlexcube integration;
        private IAdminRepository admin;

        bool USE_THIRD_PARTY_INTEGRATION = false;


        public CreditDrawdownRepository(FinTrakBankingContext _context, 
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, 
                                        IWorkflow _workflow, 
                                        IOverRideRepository _overrider, 
                                        IntegrationWithFlexcube _integration,
                                        IAdminRepository _admin
                                         )
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workflow = _workflow;
            this.overrider = _overrider;
            this.integration = _integration;
            this.admin = _admin;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;

        }

        public IEnumerable<TransactionDynamicsViewModel> GetLoanTransactionDynamics(int loanApplicationDetailId)
        {
            return (from data in context.TBL_LOAN_TRANSACTION_DYNAMICS
                    where data.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                    select new TransactionDynamicsViewModel()
                    {
                        // dynamicsId = data.DYNAMICSID,
                        dynamics = data.DYNAMICS,
                        //productId = data.TBL_TRANSACTION_DYNAMICS.PRODUCTID,
                        // loanDynamicsId = data.LOANDYNAMICSID
                    });
        }

        public WorkflowResponse LogApprovalForMessage(ForwardViewModel model, bool externalInitialization, bool saveChanges = false)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId > 0 ? model.applicationId : model.targetId;
            workflow.CompanyId = model.companyId;
            workflow.Comment = model.comment;
            workflow.ExternalInitialization = externalInitialization;
            workflow.StatusId = model.forwardAction;
            workflow.DeferredExecution = true;
            workflow.Amount = model.amount;
            
            workflow.LogActivity();

            if (saveChanges)
            {
                context.SaveChanges();
            }

            return workflow.Response;
        }

        public bool LogApproval(ForwardViewModel model, int operationId, bool externalInitialization, int ApprovalStatusId)
        {
            
                if (externalInitialization)
                {
                    workflow.StaffId = model.createdBy;
                    workflow.OperationId = operationId;
                    workflow.TargetId = model.applicationId;
                    workflow.CompanyId = model.companyId;
                    workflow.Comment = model.comment;
                    workflow.ExternalInitialization = externalInitialization;
                    workflow.StatusId = ApprovalStatusId;
                    workflow.Amount = model.amount;
                    if (model.ownerId > 0) workflow.OwnerId = model.ownerId;
                    if (model.toStaffId > 0) workflow.ToStaffId = model.toStaffId;
            }

                if (!externalInitialization)
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = ApprovalStatusId;
                    workflow.TargetId = model.applicationId;
                    workflow.Comment = model.comment;
                    workflow.OperationId = operationId;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;
                    if (model.ownerId > 0) workflow.OwnerId = model.ownerId;
                    if (model.toStaffId > 0) workflow.ToStaffId = model.toStaffId;
            }

                workflow.LogActivity();

                return context.SaveChanges() > 0;
            
        }

        public CurrentCustomerExposure GetCurrentCompanyExposure()
        {
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            CurrentCustomerExposure totalExposures = new CurrentCustomerExposure();

            exposure = context.TBL_LOAN
                    .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
                        proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

            if (exposure.Count() > 0) exposures.AddRange(exposure);

            // Same for revolving and contegent facility ...

            exposure = context.TBL_LOAN_REVOLVING
                .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                .Select(g => new CurrentCustomerExposure
                {
                    facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                    existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                    proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                    recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                    PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                    pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                    reviewDate = DateTime.Now,
                    prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                    loanStatus = "Running"
                });

            if (exposure.Count() > 0) exposures.AddRange(exposure);


            exposure = context.TBL_LOAN_CONTINGENT
                .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                .Select(g => new CurrentCustomerExposure
                {
                    facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                    existingLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    proposedLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                    reviewDate = DateTime.Now,
                    loanStatus = "Running"
                });

            if (exposure.Count() > 0) exposures.AddRange(exposure);


            totalExposures = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
                prudentialGuideline = String.Empty,
                loanStatus = String.Empty,
            };

            return totalExposures;
        }

        public WorkflowResponse GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.targetId);
                var applicationDet = context.TBL_LOAN_APPLICATION_DETAIL.Find(request.LOANAPPLICATIONDETAILID);
                var application = context.TBL_LOAN_APPLICATION.Find(applicationDet.LOANAPPLICATIONID);
                bool isContingent = false;

                // checking of company limit at availment
                //var exposure = GetCurrentCompanyExposure();
                //var proposedExposure = exposure.proposedLimit + applicationDet.APPROVEDAMOUNT;
                //var company = context.TBL_COMPANY.Find(application.COMPANYID);
                //if (proposedExposure >= company.SHAREHOLDERSFUND)
                //{
                //    throw new SecureException("Company Limit Exceeded!");
                //}

                if (application.ISLINEFACILITY == true && entity.documentProvided != null)
                {
                    if (entity?.documentProvided == true) { applicationDet.APPROVEDLINESTATUSID = 1; }
                    if (entity?.documentProvided == false) { applicationDet.APPROVEDLINESTATUSID = 2; }
                }

                if(entity.approvalStatusId != (int)ApprovalStatusEnum.Referred)
                {
                    var classifiedTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                     x.OPERATIONID == (int)entity.operationId
                     //&& x.RESPONSESTAFFID == null
                     && x.DESTINATIONOPERATIONID > 0
                     && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                     && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing)
                     //&& x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                     && x.TARGETID == entity.targetId
                    );

                    var previousTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                     x.OPERATIONID == (int)entity.operationId
                     //&& x.RESPONSESTAFFID == null
                     && x.REFEREBACKSTATEID != (int)ApprovalState.Ended
                     && x.DESTINATIONOPERATIONID == null
                     && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                     && x.TARGETID == entity.targetId
                    );

                    if (classifiedTrail != null && previousTrail == null)
                    {
                        
                        if (classifiedTrail.RESPONSESTAFFID == null)
                        {
                            if (classifiedTrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                            {
                                classifiedTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Closed;
                            }
                            else
                            {
                                classifiedTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                            classifiedTrail.APPROVALSTATEID = (short)ApprovalState.Ended;
                            classifiedTrail.RESPONSESTAFFID = entity.staffId;
                            classifiedTrail.RESPONSEDATE = DateTime.Now;
                            classifiedTrail.SYSTEMRESPONSEDATETIME = DateTime.Now;
                        }
                            
                        classifiedTrail.REFEREBACKSTATEID = (short)ApprovalState.Ended;
                        context.SaveChanges();

                        var previousTrail2 = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                         x.OPERATIONID == (int)entity.operationId
                         && x.RESPONSESTAFFID == null
                         && x.DESTINATIONOPERATIONID == null
                         && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Finishing || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                         && x.TARGETID == entity.targetId
                        );

                        if (previousTrail2 != null)
                        {

                            if (previousTrail2.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                            {
                                previousTrail2.APPROVALSTATUSID = (int)ApprovalStatusEnum.Closed;
                            }
                            else
                            {
                                previousTrail2.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                            previousTrail2.APPROVALSTATEID = (short)ApprovalState.Ended;
                            previousTrail2.RESPONSESTAFFID = entity.staffId;
                            previousTrail2.RESPONSEDATE = DateTime.Now;
                            previousTrail2.SYSTEMRESPONSEDATETIME = DateTime.Now;
                        }

                            request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                            var operationId = classifiedTrail.OPERATIONID;

                            var approvalModel = new ForwardViewModel
                            {
                                createdBy = entity.createdBy,
                                companyId = entity.companyId,
                                applicationId = request.LOAN_BOOKING_REQUESTID,
                                comment = "A request for booking needs your attention",
                                amount = request.AMOUNT_REQUESTED,
                            };
                            application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;

                            if (operationId > 0) LogApproval(approvalModel, classifiedTrail.DESTINATIONOPERATIONID ?? 0, true, (short)ApprovalStatusEnum.Pending);
                        if (request.PRODUCTID == 156) request.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            context.SaveChanges();
                            trans.Commit();

                            return workflow.Response;
                    }
                }

                var drawdowProduct = context.TBL_PRODUCT.Where(x => x.PRODUCTID == request.PRODUCTID).FirstOrDefault();
                var drawdownAmt = (request.AMOUNT_REQUESTED * (decimal)applicationDet.EXCHANGERATE);
                if (applicationDet.EXCHANGERATE == 0.0)
                {
                    drawdownAmt = request.AMOUNT_REQUESTED;
                }

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                workflow.TargetId = entity.targetId;
                workflow.Comment = entity.comment;
                workflow.OperationId = entity.operationId;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = false;
                workflow.Amount = drawdownAmt;
                workflow.BusinessUnitId = applicationDet.TBL_CUSTOMER?.BUSINESSUNTID;
                workflow.IsFromPc = entity.isFromPc;
                workflow.OwnerId = application.OWNEDBY;

                if (drawdowProduct?.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                {
                    workflow.IgnorePostApprovalReviewer = true;
                    isContingent = true;
                }


                workflow.LevelBusinessRule = new LevelBusinessRule
                {
                    Amount = drawdownAmt,
                    PepAmount = drawdownAmt,
                    Pep = application.ISPOLITICALLYEXPOSED,
                    InsiderRelated = application.ISRELATEDPARTY,
                    ProjectRelated = application.ISPROJECTRELATED,
                    OnLending = application.ISONLENDING,
                    InterventionFunds = application.ISINTERVENTIONFUNDS,
                    OrrBasedApproval = application.ISORRBASEDAPPROVAL,
                    DomiciliationNotInPlace = application.DOMICILIATIONNOTINPLACE,
                    isContingentFacility = drawdowProduct?.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability
                };

                workflow.LogActivity();

                //if (context.TBL_PRODUCT.Where(x=>x.PRODUCTID == request.PRODUCTID).FirstOrDefault()?.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                //{
                //    workflow.TerminateOnApproval = true;
                //    isContingent = true;
                //}

                context.SaveChanges();
                var isClosed = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == entity.targetId && x.OPERATIONID == entity.operationId && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Closed).Any();
                if (entity.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                    trans.Commit();
                    context.SaveChanges();
                    return workflow.Response;
                    //return 3;
                }
                else if (workflow.NewState == (int)ApprovalState.Ended && (isClosed == true || isContingent == true))
                    //&& request.CRMSVALIDATED == true) 
                        //|| (workflow.NewState == (int)ApprovalState.Ended && isContingent == true))
                {
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    var operationId = 0;
                    var restrictBooking = false;
                    var amendWorkflow = context.TBL_WORKFLOW_AMEND.Where(w => w.PRODUCTID == request.PRODUCTID).FirstOrDefault();
                    if (amendWorkflow != null)
                    {
                        if (request.PRODUCTID == 156 && (request.AMOUNT_REQUESTED <= amendWorkflow.AMOUNT)) restrictBooking = true;
                    }
                   
                   
                    if(restrictBooking == false)
                    {
                        
                        if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan)
                            operationId = (short)OperationsEnum.CommercialLoanBooking;
                        if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                            operationId = (short)OperationsEnum.ContigentLoanBooking;
                        if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                            operationId = (short)OperationsEnum.TermLoanBooking;
                        if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving)
                            operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                        if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                            operationId = (short)OperationsEnum.RevolvingLoanBooking;

                        var approvalModel = new ForwardViewModel
                        {
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            applicationId = request.LOAN_BOOKING_REQUESTID,
                            comment = "A request for booking needs your attention",
                            amount = request.AMOUNT_REQUESTED,
                            ownerId = application.OWNEDBY,
                        };

                        if (operationId > 0) LogApproval(approvalModel, operationId, true, (short)ApprovalStatusEnum.Pending);
                    }
                    
                                        
                    
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                    var loanLienDetail = context.TBL_APPLICATIONDETAIL_LIEN.FirstOrDefault(l => l.APPLICATIONDETAILID == request.LOANAPPLICATIONDETAILID && l.DELETED == false && l.ISRELEASED == false);
                    if (loanLienDetail != null)
                    {
                        var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
                        {
                            username = "model.username",//for test, real value to be passed!!!
                            passcode = "model.passCode",
                            skipAuthentication = true
                        };
                        PlaceLien(loanLienDetail.APPLICATIONDETAILID, twoFactorAuthDetails, entity.createdBy);
                    }

                    //PlaceLien(request.LOANAPPLICATIONDETAILID, new TwoFactorAutheticationViewModel() { username = "model.username", passcode = "model.passCode" });

                    context.SaveChanges();
                    trans.Commit();
                    if (operationId != (short)OperationsEnum.ContigentLoanBooking && workflow.NewState == (int)ApprovalState.Ended)
                    {
                        //workflow.Response.responseMessage += " Proceeding to CRMS Code Capture.";
                    }
                    return workflow.Response;
                    //return 0;
                }

                else if (workflow.NewState == (int)ApprovalState.Ended && isContingent == false)
                {
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    var operationId = 0;

                    if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan)
                        operationId = (short)OperationsEnum.CommercialLoanBooking;
                    if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                        operationId = (short)OperationsEnum.ContigentLoanBooking;
                    if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                        operationId = (short)OperationsEnum.TermLoanBooking;
                    if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving)
                        operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                    if (drawdowProduct.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                        operationId = (short)OperationsEnum.RevolvingLoanBooking;

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = request.LOAN_BOOKING_REQUESTID,
                        comment = "A request for booking needs your attention",
                        amount = request.AMOUNT_REQUESTED,
                        ownerId = application.OWNEDBY,
                    };

                    if (operationId > 0) LogApproval(approvalModel, operationId, true, (short)ApprovalStatusEnum.Pending);


                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                    var loanLienDetail = context.TBL_APPLICATIONDETAIL_LIEN.FirstOrDefault(l => l.APPLICATIONDETAILID == request.LOANAPPLICATIONDETAILID && l.DELETED == false && l.ISRELEASED == false);
                    if (loanLienDetail != null)
                    {
                        var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
                        {
                            username = "model.username",//for test, real value to be passed!!!
                            passcode = "model.passCode",
                            skipAuthentication = true
                        };
                        PlaceLien(loanLienDetail.APPLICATIONDETAILID, twoFactorAuthDetails, entity.createdBy);
                    }

                    //PlaceLien(request.LOANAPPLICATIONDETAILID, new TwoFactorAutheticationViewModel() { username = "model.username", passcode = "model.passCode" });

                    context.SaveChanges();
                    trans.Commit();
                    if (operationId != (short)OperationsEnum.ContigentLoanBooking && workflow.NewState == (int)ApprovalState.Ended)
                    {
                        //workflow.Response.responseMessage += " Proceeding to CRMS Code Capture.";
                    }
                    return workflow.Response;
                    //return 0;
                }

                else
                {
                    if (!isContingent && workflow.NewState == (int)ApprovalState.Ended)
                    {
                        //workflow.Response.responseMessage += " Proceeding to CRMS Code Capture.";
                        context.SaveChanges();
                        trans.Commit();
                        return workflow.Response;
                    }
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestInitiated;
                    if (request.PRODUCTID == 156) request.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    context.SaveChanges();
                    trans.Commit();
                    return workflow.Response;
                    //return 1;
                }
            }

            //return workflow.Response;
        }

        public bool setLineFacilityLegalDocumentStatus(RecommendedCollateralViewModel entity,int loanBookingRequestId, bool value)
        {
            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(loanBookingRequestId);
            if(request != null)
            {
                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(request.LOANAPPLICATIONDETAILID);
                if(facility != null)
                {
                    facility.APPROVEDLINESTATUSID = value == true ? facility.APPROVEDLINESTATUSID = 1 : facility.APPROVEDLINESTATUSID = 2;

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationUpdate,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        TARGETID = request.LOAN_BOOKING_REQUESTID,
                        DETAIL = $"Line facility Legal Document set to '{ value }' for loan application ref '{facility.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER} on facility {facility.TBL_PRODUCT.PRODUCTNAME}'",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                        APPLICATIONDATE = generalSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };
                    context.TBL_AUDIT.Add(audit);

                    return context.SaveChanges() > 0;
                }
                return false;
            }

            return false;
        }

        /*public IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation = false)
        {
            List<int> operationIds = new List<int>();
            operationIds.Add((int)OperationsEnum.CorporateDrawdownRequest);
            operationIds.Add((int)OperationsEnum.IndividualDrawdownRequest);
            operationIds.Add((int)OperationsEnum.CreditCardDrawdownRequest);
            operationIds.Add((int)OperationsEnum.RevolvingTranchDisbursement);
            operationIds.Add((int)OperationsEnum.IBLAvailmentInProgress);
            var staffs = generalSetup.GetStaffRlieved(staffId);

            List<int> levelIds = new List<int>();
            foreach(var i in operationIds) { levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, i).ToList()); }

            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CorporateDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IndividualDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CreditCardDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IBLAvailmentInProgress).ToList());

            List<CamProcessedLoanViewModel> data = new List<CamProcessedLoanViewModel>();

            data = (from req in context.TBL_LOAN_BOOKING_REQUEST
                    join d in context.TBL_LOAN_APPLICATION_DETAIL on req.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                    join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                    join coy in context.TBL_COMPANY on m.COMPANYID equals coy.COMPANYID
                    join p in context.TBL_PRODUCT on req.PRODUCTID equals p.PRODUCTID
                    join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                    join cust2 in context.TBL_CUSTOMER on req.CUSTOMERID equals cust2.CUSTOMERID into reqCust
                    join br in context.TBL_BRANCH on m.BRANCHID equals br.BRANCHID
                    join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                    from cust2 in reqCust.DefaultIfEmpty()
                    let customer = cust2 != null ? cust2 : cust
                    where operationIds.Contains(atrail.OPERATIONID)
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                            && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                            && req.ISUSED == false && atrail.RESPONSESTAFFID == null
                            && ((atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved)
                                            && (atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved)
                                            && (atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Finishing))
                            && (req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                            && ( (levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null) 
                              || (!levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && staffs.Contains((int)atrail.LOOPEDSTAFFID)))
                          //|| (isInitiation == true && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved && req.DELETED == false)

                    orderby atrail.SYSTEMARRIVALDATETIME descending

                    select new CamProcessedLoanViewModel
                    {
                        trailId = atrail.APPROVALTRAILID,
                        isProjectRelate = m.ISPROJECTRELATED,
                        loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                        bookingOperationId = req.OPERATIONID,
                        approvalTrailId = atrail.APPROVALTRAILID,
                        approvalStatusId = (short)atrail.APPROVALSTATUSID,
                        approvalStatusName = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                        loanApplicationId = m.LOANAPPLICATIONID,
                        loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                        isLineFacility = d.ISLINEFACILITY,
                        isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
                        isEmployerRelated = m.ISEMPLOYERRELATED,
                        employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == m.RELATEDEMPLOYERID).EMPLOYER_NAME,
                        applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                        applicationStatusId = m.APPLICATIONSTATUSID,
                        appraisalOperationId = m.OPERATIONID,
                        operationId = atrail.OPERATIONID,
                        requestedAmount = req.AMOUNT_REQUESTED,
                        customerId = customer.CUSTOMERID,
                        customerCode = customer.CUSTOMERCODE,
                        systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                        customerName = customer.FIRSTNAME + " " + customer.MIDDLENAME + " " + customer.LASTNAME,
                        customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                        customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                        isRelatedParty = m.ISRELATEDPARTY,
                        customerSensitivityLevelId = customer.CUSTOMERSENSITIVITYLEVELID,
                        customerOccupation = customer.OCCUPATION,
                        customerType = customer.TBL_CUSTOMER_TYPE.NAME,
                        isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                        isInvestmentGrade = m.ISINVESTMENTGRADE,
                        companyId = m.COMPANYID,
                        branchId = m.BRANCHID,
                        branchName = m.TBL_BRANCH.BRANCHNAME,
                        subSectorId = d.SUBSECTORID,
                        subSectorName = d.TBL_SUB_SECTOR.NAME,
                        sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                        applicationTenor = m.APPLICATIONTENOR,
                        effectiveDate = (DateTime)d.EFFECTIVEDATE,
                        expiryDate = (DateTime)d.EXPIRYDATE,
                        relationshipOfficerId = m.RELATIONSHIPOFFICERID,
                        relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
                        relationshipManagerId = m.RELATIONSHIPMANAGERID,
                        relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

                        currencyId = d.CURRENCYID,
                        currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                        exchangeRate = d.EXCHANGERATE,
                        loanTypeId = m.LOANAPPLICATIONTYPEID,
                        loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                        camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                        productId = req.PRODUCTID,
                        productTypeId = p.PRODUCTTYPEID,
                        productTypeName = p.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        productName = p.PRODUCTNAME,
                        productClassId = p.PRODUCTCLASSID,
                        productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                        productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                        misCode = m.MISCODE,
                        teamMisCode = m.TEAMMISCODE,
                        casaAccountId = req.CASAACCOUNTID,
                        casaAccountId2 = req.CASAACCOUNTID2,

                        interestRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                        approvedDate = m.APPROVEDDATE,
                        groupApprovedAmount = m.APPROVEDAMOUNT,
                        approvedTenor = d.APPROVEDTENOR,
                        createdBy = m.OWNEDBY,
                        newApplicationDate = m.APPLICATIONDATE,
                        dateTimeCreated = d.DATETIMECREATED,
                        availmentDate = m.AVAILMENTDATE,
                        requestDate = req.DATETIMECREATED,
                        apiRequestId = m.APIREQUESTID,
                        toStaffId = atrail.TOSTAFFID,
                        documentProvided = d.APPROVEDLINESTATUSID,
                        //staffId = (atrail.LOOPEDSTAFFID != null ? atrail.LOOPEDSTAFFID : atrail.TOSTAFFID),
                        divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                        divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == d.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                    }).ToList();

            data = data.Where(x => x.applicationReferenceNumber != "-")
              .GroupBy(p => p.loanBookingRequestId).Select(l => l.OrderByDescending(t => t.systemArrivalDateTime).FirstOrDefault())
                  .ToList();

            foreach (var item in data)
            {
                var casa1 = context.TBL_CASA.Find(item.casaAccountId);
                var casa2 = context.TBL_CASA.Find(item.casaAccountId2);
                if (casa1 != null) item.accountNumber = casa1.PRODUCTACCOUNTNUMBER;
                if (casa2 != null) item.accountNumber2 = casa2.PRODUCTACCOUNTNUMBER;

                var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                    item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                if (item.disapprovedCount > 0)
                    item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED);

                item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);

                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                if (disbursedLoan.Any())
                {
                    item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                }
            }

            return data.ToList();
        } */

        public IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation = false)
        {
            var operationIds = new List<int>(5)
            {
                (int)OperationsEnum.CorporateDrawdownRequest,
                (int)OperationsEnum.IndividualDrawdownRequest,
                (int)OperationsEnum.CreditCardDrawdownRequest,
                (int)OperationsEnum.RevolvingTranchDisbursement,
                (int)OperationsEnum.IBLAvailmentInProgress
            };
            var staffs = generalSetup.GetStaffRlieved(staffId);

            List<int> levelIds = new List<int>();
            foreach (var i in operationIds) { levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, i).ToList()); }

            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CorporateDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IndividualDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CreditCardDrawdownRequest).ToList());
            levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IBLAvailmentInProgress).ToList());

            List<CamProcessedLoanViewModel> data = new List<CamProcessedLoanViewModel>();

            data = (from req in context.TBL_LOAN_BOOKING_REQUEST
                    join d in context.TBL_LOAN_APPLICATION_DETAIL on req.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                    join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                    join coy in context.TBL_COMPANY on m.COMPANYID equals coy.COMPANYID
                    join p in context.TBL_PRODUCT on req.PRODUCTID equals p.PRODUCTID
                    join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                    join cust2 in context.TBL_CUSTOMER on req.CUSTOMERID equals cust2.CUSTOMERID into reqCust
                    join br in context.TBL_BRANCH on m.BRANCHID equals br.BRANCHID
                    join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                    from cust2 in reqCust.DefaultIfEmpty()
                    let customer = cust2 != null ? cust2 : cust
                    where operationIds.Contains(atrail.OPERATIONID)
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                            && m.COMPANYID == companyId
                            && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                            && req.ISUSED == false && atrail.RESPONSESTAFFID == null
                            && ((atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved)
                                            && (atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved)
                                            && (atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Finishing))
                            && (req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                            && ((levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null)
                              || (!levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && staffs.Contains((int)atrail.LOOPEDSTAFFID)))
                    //|| (isInitiation == true && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved && req.DELETED == false)

                    orderby atrail.SYSTEMARRIVALDATETIME descending

                    select new CamProcessedLoanViewModel
                    {
                        trailId = atrail.APPROVALTRAILID,
                        isProjectRelate = m.ISPROJECTRELATED,
                        loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                        bookingOperationId = req.OPERATIONID,
                        approvalTrailId = atrail.APPROVALTRAILID,
                        approvalStatusId = (short)atrail.APPROVALSTATUSID,
                        approvalStatusName = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                        loanApplicationId = m.LOANAPPLICATIONID,
                        loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                        isLineFacility = d.ISLINEFACILITY,
                        isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
                        isEmployerRelated = m.ISEMPLOYERRELATED,
                        employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == m.RELATEDEMPLOYERID).EMPLOYER_NAME,
                        applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                        applicationStatusId = m.APPLICATIONSTATUSID,
                        appraisalOperationId = m.OPERATIONID,
                        operationId = atrail.OPERATIONID,
                        requestedAmount = req.AMOUNT_REQUESTED,
                        customerId = customer.CUSTOMERID,
                        customerCode = customer.CUSTOMERCODE,
                        systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                        customerName = customer.FIRSTNAME + " " + customer.MIDDLENAME + " " + customer.LASTNAME,
                        customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                        customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                        isRelatedParty = m.ISRELATEDPARTY,
                        customerSensitivityLevelId = customer.CUSTOMERSENSITIVITYLEVELID,
                        customerOccupation = customer.OCCUPATION,
                        customerType = customer.TBL_CUSTOMER_TYPE.NAME,
                        isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                        isInvestmentGrade = m.ISINVESTMENTGRADE,
                        companyId = m.COMPANYID,
                        branchId = m.BRANCHID,
                        branchName = m.TBL_BRANCH.BRANCHNAME,
                        subSectorId = d.SUBSECTORID,
                        subSectorName = d.TBL_SUB_SECTOR.NAME,
                        sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                        applicationTenor = m.APPLICATIONTENOR,
                        effectiveDate = (DateTime)d.EFFECTIVEDATE,
                        expiryDate = (DateTime)d.EXPIRYDATE,
                        relationshipOfficerId = m.RELATIONSHIPOFFICERID,
                        relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
                        relationshipManagerId = m.RELATIONSHIPMANAGERID,
                        relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

                        currencyId = d.CURRENCYID,
                        currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                        exchangeRate = d.EXCHANGERATE,
                        loanTypeId = m.LOANAPPLICATIONTYPEID,
                        loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                        camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                        productId = req.PRODUCTID,
                        productTypeId = p.PRODUCTTYPEID,
                        productTypeName = p.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        productName = p.PRODUCTNAME,
                        productClassId = p.PRODUCTCLASSID,
                        productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                        productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                        misCode = m.MISCODE,
                        teamMisCode = m.TEAMMISCODE,
                        casaAccountId = req.CASAACCOUNTID,
                        casaAccountId2 = req.CASAACCOUNTID2,

                        interestRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                        approvedDate = m.APPROVEDDATE,
                        groupApprovedAmount = m.APPROVEDAMOUNT,
                        approvedTenor = d.APPROVEDTENOR,
                        createdBy = m.OWNEDBY,
                        newApplicationDate = m.APPLICATIONDATE,
                        dateTimeCreated = d.DATETIMECREATED,
                        availmentDate = m.AVAILMENTDATE,
                        requestDate = req.DATETIMECREATED,
                        apiRequestId = m.APIREQUESTID,
                        toStaffId = atrail.TOSTAFFID,
                        documentProvided = d.APPROVEDLINESTATUSID,
                        //staffId = (atrail.LOOPEDSTAFFID != null ? atrail.LOOPEDSTAFFID : atrail.TOSTAFFID),
                        divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                        divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == d.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                    }).ToList();

            data = data.Where(x => x.applicationReferenceNumber != "-")
              .GroupBy(p => p.loanBookingRequestId).Select(l => l.OrderByDescending(t => t.systemArrivalDateTime).FirstOrDefault())
                  .ToList();

            foreach (var item in data)
            {
                var casa1 = context.TBL_CASA.Find(item.casaAccountId);
                var casa2 = context.TBL_CASA.Find(item.casaAccountId2);
                if (casa1 != null) item.accountNumber = casa1.PRODUCTACCOUNTNUMBER;
                if (casa2 != null) item.accountNumber2 = casa2.PRODUCTACCOUNTNUMBER;

                var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                    item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                if (item.disapprovedCount > 0)
                    item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED);

                item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);

                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                if (disbursedLoan.Any())
                {
                    item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                }
            }

            return data.ToList();
        }

        public int GetDrawdownOperationId(int applicationDetailId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(applicationDetailId);

            var requestedProduct = context.TBL_PRODUCT.Find(loanApplicationDetails.APPROVEDPRODUCTID);

            var operationId = 0;
            if (requestedProduct.PRODUCTCLASSID == (short)ProductClassEnum.Creditcards)
            {
                operationId = (short)OperationsEnum.CreditCardDrawdownRequest;
            }
            else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
            {
                if (requestedProduct.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased)
                {
                    operationId = (short)OperationsEnum.CorporateDrawdownRequest;
                }
                operationId = (short)OperationsEnum.IndividualDrawdownRequest;
            }
            else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
            {
                if (GetRevolvingTrancheDisbursementOperationId(applicationDetailId))
                {
                    operationId = (short)OperationsEnum.RevolvingTranchDisbursement;
                }
                else
                {
                    operationId = (short)OperationsEnum.CorporateDrawdownRequest;
                }
                
            }

            return operationId;
        }

        private bool GetRevolvingTrancheDisbursementOperationId(int applicationDetailId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(applicationDetailId);

            if (loanApplicationDetails.ISLINEFACILITY == true && loanApplicationDetails.APPROVEDLINESTATUSID == (short)LegalDocumentStatusEnum.Conditional )
            {
                return true;
            }
             return false;
        }

        private IEnumerable<CamProcessedLoanViewModel> AvailedLoanApplicationsDetails(int companyId, int staffId, int branchId)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);

            var data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                         where a.COMPANYID == companyId && d.DELETED == false
                         && a.OWNEDBY == staffId
                         && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                         && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                         && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                         && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                         && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                         //&& a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.BookingRequestInitiated
                         orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
                         select new CamProcessedLoanViewModel
                         {
                             //approvalStatusId = (short)atrail.APPROVALSTATUSID,
                             loanBookingRequestId = 0,
                             approvalTrailId = 0,
                             appraisalOperationId = a.OPERATIONID,
                             //bookingAmountRequested = r.AMOUNT_REQUESTED,
                             requestedAmount = 0,
                             loanApplicationId = a.LOANAPPLICATIONID,
                             loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             customerId = d.CUSTOMERID,
                             
                             //customerCode = from cust in context.TBL_CUSTOMER where cust.CUSTOMERID == d.CUSTOMERID select cust.CUSTOMERCODE.FirstOrDefault( ,
                             customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                             customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
                             customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                             customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                             isRelatedParty = a.ISRELATEDPARTY,
                             customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                             customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                             customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                             operationId = a.OPERATIONID,
                             isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                             isInvestmentGrade = a.ISINVESTMENTGRADE,
                             productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                             companyId = a.COMPANYID,
                             branchId = a.BRANCHID,
                             branchName = a.TBL_BRANCH.BRANCHNAME,
                             subSectorId = d.SUBSECTORID,
                             subSectorName = d.TBL_SUB_SECTOR.NAME,
                             sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                             applicationTenor = a.APPLICATIONTENOR,
                             effectiveDate = (DateTime)d.EFFECTIVEDATE,
                             expiryDate = (DateTime)d.EXPIRYDATE,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

                             currencyId = d.CURRENCYID,
                             currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                             isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                             exchangeRate = d.EXCHANGERATE,
                             loanTypeId = a.LOANAPPLICATIONTYPEID,
                             loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                             camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                             productId = d.APPROVEDPRODUCTID,
                             productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                             productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                             productName = d.TBL_PRODUCT.PRODUCTNAME,
                             productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                             productClassId = a.PRODUCTCLASSID,
                             misCode = a.MISCODE,
                             teamMisCode = a.TEAMMISCODE,
                             casaAccountId = d.CASAACCOUNTID,

                             interestRate = d.APPROVEDINTERESTRATE,
                             submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                             approvedAmount = d.APPROVEDAMOUNT,
                             approvedDate = a.APPROVEDDATE,
                             groupApprovedAmount = a.APPROVEDAMOUNT,
                             approvedTenor = d.APPROVEDTENOR,
                             createdBy = a.OWNEDBY,
                             newApplicationDate = a.APPLICATIONDATE,
                             dateTimeCreated = d.DATETIMECREATED,
                             availmentDate = a.AVAILMENTDATE,
                             systemCurrentDate = systemDate,
                             isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                             loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),

                             //availableAmount = 
                         }).ToList();

            var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                        join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                        //join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID

                        where m.COMPANYID == companyId && d.DELETED == false
                        && ((m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.AvailmentCompleted)
                        || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestInitiated)
                        || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestCompleted)
                        || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingInProgress)
                        || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingCompleted))
                        && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
                        && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                        && m.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                        && m.BRANCHID == branchId
                        join r in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                        join atrail in context.TBL_APPROVAL_TRAIL on r.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                        where r.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved
                        && r.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved && atrail.RESPONSESTAFFID == null
                        && ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred) && (atrail.LOOPEDSTAFFID == staffId))
                        //&& ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing) || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending))
                        orderby atrail.SYSTEMARRIVALDATETIME descending, m.DATETIMECREATED descending
                        //orderby m.AVAILMENTDATE descending, m.DATETIMECREATED descending
                        select new CamProcessedLoanViewModel
                        {
                            loanBookingRequestId = r.LOAN_BOOKING_REQUESTID,
                            approvalTrailId = atrail.APPROVALTRAILID,
                            //bookingAmountRequested = r.AMOUNT_REQUESTED,
                            requestedAmount = r.AMOUNT_REQUESTED,
                            //approvalStatusId = (short) m.APPROVALSTATUSID,
                            appraisalOperationId = m.OPERATIONID,
                            loanApplicationId = m.LOANAPPLICATIONID,
                            loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                            applicationStatusId = m.APPLICATIONSTATUSID,
                            customerId = d.CUSTOMERID,
                            systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                            //customerCode = from cust in context.TBL_CUSTOMER where cust.CUSTOMERID == d.CUSTOMERID select cust.CUSTOMERCODE.FirstOrDefault( ,
                            customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                            customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                            isRelatedParty = m.ISRELATEDPARTY,
                            customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                            customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                            customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            operationId = atrail.OPERATIONID,
                            isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                            isInvestmentGrade = m.ISINVESTMENTGRADE,
                            productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                            companyId = m.COMPANYID,
                            branchId = m.BRANCHID,
                            branchName = m.TBL_BRANCH.BRANCHNAME,
                            subSectorId = d.SUBSECTORID,
                            subSectorName = d.TBL_SUB_SECTOR.NAME,
                            sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            applicationTenor = m.APPLICATIONTENOR,
                            effectiveDate = (DateTime)d.EFFECTIVEDATE,
                            expiryDate = (DateTime)d.EXPIRYDATE,
                            relationshipOfficerId = m.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
                            relationshipManagerId = m.RELATIONSHIPMANAGERID,
                            relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

                            currencyId = d.CURRENCYID,
                            currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                            isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                            exchangeRate = d.EXCHANGERATE,
                            loanTypeId = m.LOANAPPLICATIONTYPEID,
                            loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                            productId = d.APPROVEDPRODUCTID,
                            productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                            productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            productName = d.TBL_PRODUCT.PRODUCTNAME,
                            productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                            productClassId = m.PRODUCTCLASSID,
                            misCode = m.MISCODE,
                            teamMisCode = m.TEAMMISCODE,
                            casaAccountId = r.CASAACCOUNTID,

                            interestRate = d.APPROVEDINTERESTRATE,
                            submittedForAppraisal = m.SUBMITTEDFORAPPRAISAL,
                            approvedAmount = d.APPROVEDAMOUNT,
                            approvedDate = m.APPROVEDDATE,
                            groupApprovedAmount = m.APPROVEDAMOUNT,
                            approvedTenor = d.APPROVEDTENOR,
                            createdBy = m.OWNEDBY,
                            newApplicationDate = m.APPLICATIONDATE,
                            dateTimeCreated = d.DATETIMECREATED,
                            availmentDate = m.AVAILMENTDATE,
                            systemCurrentDate = systemDate,
                            isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                            loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0,

                            //approvalStatusId = (short)m.APPROVALSTATUSID,
                            approvalStatusId = (short)atrail.APPROVALSTATUSID,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),

                        }).ToList();

            data = data.Union(data2).ToList();

            foreach (var item in data)
            {
                var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                switch (item.productTypeId)
                {
                    case (short)LoanProductTypeEnum.TermLoan:
                        decimal customerAvailableAmount = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount = customerAvailableAmount + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount;
                        break;
                    case (short)LoanProductTypeEnum.CommercialLoan:
                        decimal customerAvailableAmount2 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount2 = customerAvailableAmount2 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount2;
                        break;
                    case (short)LoanProductTypeEnum.SelfLiquidating:
                        decimal customerAvailableAmount3 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount3 = customerAvailableAmount3 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount3;
                        break;
                    case (short)LoanProductTypeEnum.RevolvingLoan:
                        decimal overdraftBal = 0;
                        foreach (var overdraft in overdrafts)
                        {
                            if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                        break;
                    case (short)LoanProductTypeEnum.ContingentLiability:
                        decimal contingentBal = 0;
                        foreach (var contingent in contingents)
                        {
                            if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - contingentBal;
                        break;
                    case (short)LoanProductTypeEnum.SyndicatedTermLoan:
                        decimal customerAvailableAmount4 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount3 = customerAvailableAmount4 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount4;
                        break;
                }

                if (item.effectiveDate == null) item.effectiveDate = item.availmentDate;
                if (item.expiryDate == null && item.effectiveDate != null) item.expiryDate = item.effectiveDate.Value.AddDays(item.approvedTenor);
            }

            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetGlobalEmployerLoansDueForInitiateBooking(int companyId, string searchString)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);
            var data2 = new List<CamProcessedLoanViewModel>();
            searchString = searchString.ToLower().Trim();

            data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                     join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                     join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                     let employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == a.RELATEDEMPLOYERID).EMPLOYER_NAME
                     where a.COMPANYID == companyId && d.DELETED == false
                     //&& staffIds.Contains(a.OWNEDBY)
                     && a.ISEMPLOYERRELATED
                     &&
                     (
                     a.APPLICATIONREFERENCENUMBER == searchString
                     || d.TBL_CUSTOMER.FIRSTNAME.ToLower().Contains(searchString)
                     || d.TBL_CUSTOMER.MIDDLENAME.ToLower().Contains(searchString)
                     || d.TBL_CUSTOMER.LASTNAME.ToLower().Contains(searchString)
                     || employer.ToLower().Contains(searchString)
                     )
                     && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                     && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                     && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                     && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                     && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                     orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
                     select new CamProcessedLoanViewModel
                     {
                         loanBookingRequestId = 0,
                         approvalTrailId = 0,
                         isLineFacility = d.ISLINEFACILITY,
                         isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
                         isLineMaintained = a.APPROVEDLINESTATUSID != null,
                         customerTypeId = (int)context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == d.CUSTOMERID).Select(s => s.CUSTOMERTYPEID).FirstOrDefault(),
                         isEmployerRelated = a.ISEMPLOYERRELATED,
                         employer = employer,
                         appraisalOperationId = a.OPERATIONID,
                         requestedAmount = 0,
                         loanApplicationId = a.LOANAPPLICATIONID,
                         loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                         applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                         applicationStatusId = a.APPLICATIONSTATUSID,
                         customerId = d.CUSTOMERID,
                         customerCode = d.TBL_CUSTOMER.CUSTOMERCODE,
                         customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                         customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
                         customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                         customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                         isRelatedParty = a.ISRELATEDPARTY,
                         customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                         customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                         customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                         operationId = a.OPERATIONID,
                         isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                         isInvestmentGrade = a.ISINVESTMENTGRADE,
                         productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                         companyId = a.COMPANYID,
                         branchId = a.BRANCHID,
                         branchName = a.TBL_BRANCH.BRANCHNAME,
                         subSectorId = d.SUBSECTORID,
                         subSectorName = d.TBL_SUB_SECTOR.NAME,
                         sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                         applicationTenor = a.APPLICATIONTENOR,
                         effectiveDate = (DateTime)d.EFFECTIVEDATE,
                         expiryDate = (DateTime)d.EXPIRYDATE,
                         relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                         relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                         relationshipManagerId = a.RELATIONSHIPMANAGERID,
                         relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

                         currencyId = d.CURRENCYID,
                         currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                         isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                         exchangeRate = d.EXCHANGERATE,
                         loanTypeId = a.LOANAPPLICATIONTYPEID,
                         loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                         camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                         productId = d.APPROVEDPRODUCTID,
                         productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                         productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                         productName = d.TBL_PRODUCT.PRODUCTNAME,
                         productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                         productClassId = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,
                         misCode = a.MISCODE,
                         teamMisCode = a.TEAMMISCODE,
                         casaAccountId = d.CASAACCOUNTID,

                         interestRate = d.APPROVEDINTERESTRATE,
                         submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                         approvedAmount = d.APPROVEDAMOUNT,
                         approvedDate = a.APPROVEDDATE,
                         groupApprovedAmount = a.APPROVEDAMOUNT,
                         approvedTenor = d.APPROVEDTENOR,
                         createdBy = a.OWNEDBY,
                         newApplicationDate = a.APPLICATIONDATE,
                         dateTimeCreated = d.DATETIMECREATED,
                         availmentDate = a.AVAILMENTDATE,
                         systemCurrentDate = systemDate,
                         isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                         loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                         approvalStatusId = (short)a.APPROVALSTATUSID,
                         apiRequestId = a.APIREQUESTID,
                         approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                     }).ToList();

            var data = data2;

            data = PerformNecessaryDrawdownInitiationCalculations(data);
            return data;
        }

        public List<CamProcessedLoanViewModel> PerformNecessaryDrawdownInitiationCalculations(List<CamProcessedLoanViewModel> data)
        {
            foreach (var item in data)
            {
                var adequateLCIssuanceIds = context.TBL_LC_ISSUANCE.Where(t => t.DELETED == false && t.CUSTOMERID == item.customerId && (t.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted)).Select(t => t.LCISSUANCEID).ToList();
                var lcIFFRequests = context.TBL_LC_ISSUANCE.Where(l => l.DELETED == false && l.FUNDSOURCEID == (int)LCFundSource.IFF).ToList();
                var lcapprovedLCIFFs = lcIFFRequests.Where(i => adequateLCIssuanceIds.Contains(i.LCISSUANCEID)).Select(i => new { i.FUNDSOURCEDETAILS, i.LETTEROFCREDITAMOUNT }).ToList();
                var lcapprovedLCIFFsRecords = lcapprovedLCIFFs.Where(i => i.FUNDSOURCEDETAILS == item.loanApplicationId).ToList();
                var lcApprovedAmounts = lcapprovedLCIFFsRecords.Count() > 0 ? lcapprovedLCIFFsRecords?.Sum(i => i.LETTEROFCREDITAMOUNT) : 0;
                var product = context.TBL_PRODUCT.Find(item.productId);

                var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && r.DELETED == false);
                var test = requests.ToList();
                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                var disbursedOverdraft = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                var disbursedContingent = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                //requests = requests.Where(x => x.APPROVEDLINESTATUSID != null && !disbursedLoan.Select(c => c.LOAN_BOOKING_REQUESTID).Contains(x.LOAN_BOOKING_REQUESTID));

                //item.operationId = GetDrawdownOperationId(item.loanApplicationDetailId);
                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                { item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED); }

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                //{ item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
                { item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED); }

                if (requests.Where(n => (n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending) && n.APPROVEDLINESTATUSID == null).Count() > 0)
                //{ item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
                {
                    var validRequests = requests.Where(n => (n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending) && n.APPROVEDLINESTATUSID == null).ToList();
                    item.allRequestAmount = validRequests.Sum(s => s.AMOUNT_REQUESTED);
                    if (product.ISFACILITYLINE == true || item.isLineFacility == true)
                    {
                        if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                        {
                            if (disbursedOverdraft.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedOverdraft.Sum(x => x.OVERDRAFTLIMIT);
                        }
                        if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                        {
                            if (disbursedContingent.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedContingent.Sum(x => x.CONTINGENTAMOUNT);
                        }
                        else
                        {
                            if (disbursedLoan.Count() > 0) item.allRequestAmount = item.allRequestAmount - disbursedLoan.Sum(x => x.PRINCIPALAMOUNT);
                        }
                    }
                }

                item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                if (item.disapprovedCount > 0)
                { item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED); }


                if (disbursedLoan.Any())
                {
                    item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                }


                //item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);
                item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount);
                if (lcApprovedAmounts > 0)
                {
                    item.customerAvailableAmount -= lcApprovedAmounts;
                }
            }

            data = (from a in data where ((a.customerAvailableAmount > 0) || (a.customerAvailableAmount == null)) select a).ToList();

            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId, int customerId, bool getAll = false)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);
            var staffIds = generalSetup.GetStaffRlieved(staffId);
            var data2 = new List<CamProcessedLoanViewModel>();

            if (getAll)
            {
                 data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                             join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                             join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                             where a.COMPANYID == companyId && d.DELETED == false
                             //&& staffIds.Contains(a.OWNEDBY)
                             && customerId == d.CUSTOMERID
                             && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                             && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                             && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                             && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                             orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
                             select new CamProcessedLoanViewModel
                             {
                                 loanBookingRequestId = 0,
                                 approvalTrailId = 0,
                                 isProjectRelate = a.ISPROJECTRELATED,
                                 isLineFacility = d.ISLINEFACILITY,
                                 isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
                                 isLineMaintained = d.APPROVEDLINESTATUSID != null,
                                 customerTypeId = (int)context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == d.CUSTOMERID).Select(s => s.CUSTOMERTYPEID).FirstOrDefault(),
                                 appraisalOperationId = a.OPERATIONID,
                                 requestedAmount = 0,
                                 loanApplicationId = a.LOANAPPLICATIONID,
                                 loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                 applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                 applicationStatusId = a.APPLICATIONSTATUSID,
                                 customerId = d.CUSTOMERID,
                                 customerCode = d.TBL_CUSTOMER.CUSTOMERCODE,
                                 customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                                 customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
                                 customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                 customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                                 isRelatedParty = a.ISRELATEDPARTY,
                                 customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                 customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                                 customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                 operationId = a.OPERATIONID,
                                 isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                                 isInvestmentGrade = a.ISINVESTMENTGRADE,
                                 productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                 companyId = a.COMPANYID,
                                 branchId = a.BRANCHID,
                                 branchName = a.TBL_BRANCH.BRANCHNAME,
                                 subSectorId = d.SUBSECTORID,
                                 subSectorName = d.TBL_SUB_SECTOR.NAME,
                                 sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                 applicationTenor = a.APPLICATIONTENOR,
                                 effectiveDate = (DateTime)d.EFFECTIVEDATE,
                                 expiryDate = (DateTime)d.EXPIRYDATE,
                                 relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                 relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                 relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                 relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

                                 currencyId = d.CURRENCYID,
                                 currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                 isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                                 exchangeRate = d.EXCHANGERATE,
                                 loanTypeId = a.LOANAPPLICATIONTYPEID,
                                 loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                 camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                                 productId = d.APPROVEDPRODUCTID,
                                 productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                                 productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                 productName = d.TBL_PRODUCT.PRODUCTNAME,
                                 productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                                 productClassId = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,
                                 misCode = a.MISCODE,
                                 teamMisCode = a.TEAMMISCODE,
                                 casaAccountId = d.CASAACCOUNTID,

                                 interestRate = d.APPROVEDINTERESTRATE,
                                 submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                                 approvedAmount = d.APPROVEDAMOUNT,
                                 approvedDate = a.APPROVEDDATE,
                                 groupApprovedAmount = a.APPROVEDAMOUNT,
                                 approvedTenor = d.APPROVEDTENOR,
                                 createdBy = a.OWNEDBY,
                                 newApplicationDate = a.APPLICATIONDATE,
                                 dateTimeCreated = d.DATETIMECREATED,
                                 availmentDate = a.AVAILMENTDATE,
                                 systemCurrentDate = systemDate,
                                 isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                                 loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                                 approvalStatusId = (short)a.APPROVALSTATUSID,
                                 apiRequestId = a.APIREQUESTID,
                                 approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                             }).ToList();
            }
            else
            {
                 data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                             join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                             join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                             where a.COMPANYID == companyId && d.DELETED == false
                             && staffIds.Contains(a.OWNEDBY)
                             && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                             && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                             && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                             && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                             orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
                             select new CamProcessedLoanViewModel
                             {
                                 loanBookingRequestId = 0,
                                 approvalTrailId = 0,
                                 isLineFacility = d.ISLINEFACILITY,
                                 isProjectRelate = a.ISPROJECTRELATED,
                                 isLineFacilityString = d.ISLINEFACILITY.HasValue ? d.ISLINEFACILITY.Value ? "Yes" : "No" : "No",
                                 isLineMaintained = d.APPROVEDLINESTATUSID != null,
                                 customerTypeId = (int)context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == d.CUSTOMERID).Select(s => s.CUSTOMERTYPEID).FirstOrDefault(),
                                 appraisalOperationId = a.OPERATIONID,
                                 requestedAmount = 0,
                                 loanApplicationId = a.LOANAPPLICATIONID,
                                 loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                 applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                 applicationStatusId = a.APPLICATIONSTATUSID,
                                 customerId = d.CUSTOMERID,
                                 customerCode = d.TBL_CUSTOMER.CUSTOMERCODE,
                                 customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                                 customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
                                 customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                 customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                                 isRelatedParty = a.ISRELATEDPARTY,
                                 customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                 customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                                 customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                 operationId = a.OPERATIONID,
                                 isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                                 isInvestmentGrade = a.ISINVESTMENTGRADE,
                                 productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                 companyId = a.COMPANYID,
                                 branchId = a.BRANCHID,
                                 branchName = a.TBL_BRANCH.BRANCHNAME,
                                 subSectorId = d.SUBSECTORID,
                                 subSectorName = d.TBL_SUB_SECTOR.NAME,
                                 sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                 applicationTenor = a.APPLICATIONTENOR,
                                 effectiveDate = (DateTime)d.EFFECTIVEDATE,
                                 expiryDate = (DateTime)d.EXPIRYDATE,
                                 relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                 relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                 relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                 relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

                                 currencyId = d.CURRENCYID,
                                 currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                 isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                                 exchangeRate = d.EXCHANGERATE,
                                 loanTypeId = a.LOANAPPLICATIONTYPEID,
                                 loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                 camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                                 productId = d.APPROVEDPRODUCTID,
                                 productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                                 productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                 productName = d.TBL_PRODUCT.PRODUCTNAME,
                                 productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                                 productClassId = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSID,
                                 misCode = a.MISCODE,
                                 teamMisCode = a.TEAMMISCODE,
                                 casaAccountId = d.CASAACCOUNTID,

                                 interestRate = d.APPROVEDINTERESTRATE,
                                 submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                                 approvedAmount = d.APPROVEDAMOUNT,
                                 approvedDate = a.APPROVEDDATE,
                                 groupApprovedAmount = a.APPROVEDAMOUNT,
                                 approvedTenor = d.APPROVEDTENOR,
                                 createdBy = a.OWNEDBY,
                                 newApplicationDate = a.APPLICATIONDATE,
                                 dateTimeCreated = d.DATETIMECREATED,
                                 availmentDate = a.AVAILMENTDATE,
                                 systemCurrentDate = systemDate,
                                 isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                                 loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                                 approvalStatusId = (short)a.APPROVALSTATUSID,
                                 apiRequestId = a.APIREQUESTID,
                                 approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                             }).ToList();
            }

            var data = data2;


            //var referredItem = GetBookingRequestAwaitingApproval(staffId, companyId, true).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Referred).ToList();
            // data.AddRange(referredItem);

            data = PerformNecessaryDrawdownInitiationCalculations(data);
            return data;

        }

        public IEnumerable<CamProcessedLoanViewModel> getApplicationsToBeAdhocApprovedForInitiateBooking(int companyId, int staffId, int branchId)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);

            //IEnumerable<CamProcessedLoanViewModel> data2;

            var data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                         where a.COMPANYID == companyId && d.DELETED == false
                         && a.OWNEDBY == staffId
                         && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                         && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress
                         && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.OfferLetterReviewInProgress
                         && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                         && a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                         //&& a.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.BookingRequestInitiated
                         orderby a.AVAILMENTDATE descending, a.DATETIMECREATED descending
                         select new CamProcessedLoanViewModel
                         {
                             //approvalStatusId = (short)atrail.APPROVALSTATUSID,
                             loanBookingRequestId = 0,
                             approvalTrailId = 0,
                             appraisalOperationId = a.OPERATIONID,
                             //bookingAmountRequested = r.AMOUNT_REQUESTED,
                             requestedAmount = 0,
                             loanApplicationId = a.LOANAPPLICATIONID,
                             loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             customerId = d.CUSTOMERID,
                             //customerCode = from cust in context.TBL_CUSTOMER where cust.CUSTOMERID == d.CUSTOMERID select cust.CUSTOMERCODE.FirstOrDefault( ,
                             customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                             customerGroupId = a.CUSTOMERGROUPID.HasValue ? a.CUSTOMERGROUPID : 0,
                             customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                             customerGroupCode = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                             isRelatedParty = a.ISRELATEDPARTY,
                             customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                             customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                             customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                             operationId = a.OPERATIONID,
                             isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                             isInvestmentGrade = a.ISINVESTMENTGRADE,
                             productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                             companyId = a.COMPANYID,
                             branchId = a.BRANCHID,
                             branchName = a.TBL_BRANCH.BRANCHNAME,
                             subSectorId = d.SUBSECTORID,
                             subSectorName = d.TBL_SUB_SECTOR.NAME,
                             sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                             applicationTenor = a.APPLICATIONTENOR,
                             effectiveDate = (DateTime)d.EFFECTIVEDATE,
                             expiryDate = (DateTime)d.EXPIRYDATE,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

                             currencyId = d.CURRENCYID,
                             currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                             isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
                             exchangeRate = d.EXCHANGERATE,
                             loanTypeId = a.LOANAPPLICATIONTYPEID,
                             loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                             camReference = a.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                             productId = d.APPROVEDPRODUCTID,
                             productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                             productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                             productName = d.TBL_PRODUCT.PRODUCTNAME,
                             productClassProcessId = a.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                             productClassId = a.PRODUCTCLASSID,
                             misCode = a.MISCODE,
                             teamMisCode = a.TEAMMISCODE,
                             casaAccountId = d.CASAACCOUNTID,

                             interestRate = d.APPROVEDINTERESTRATE,
                             submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                             approvedAmount = d.APPROVEDAMOUNT,
                             approvedDate = a.APPROVEDDATE,
                             groupApprovedAmount = a.APPROVEDAMOUNT,
                             approvedTenor = d.APPROVEDTENOR,
                             createdBy = a.OWNEDBY,
                             newApplicationDate = a.APPLICATIONDATE,
                             dateTimeCreated = d.DATETIMECREATED,
                             availmentDate = a.AVAILMENTDATE,
                             systemCurrentDate = systemDate,
                             isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                             loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),

                             //availableAmount = 
                         }).ToList();

            //var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
            //            join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
            //            //join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID

            //            where m.COMPANYID == companyId && d.DELETED == false
            //            && ((m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.AvailmentCompleted)
            //            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestInitiated)
            //            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestCompleted)
            //            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingInProgress)
            //            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingCompleted))
            //            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
            //            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
            //            && m.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //            && m.BRANCHID == branchId
            //            join r in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
            //            join atrail in context.TBL_APPROVAL_TRAIL on r.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
            //            where r.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved
            //            && r.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved && atrail.RESPONSESTAFFID == null
            //            && ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred) && (atrail.LOOPEDSTAFFID == staffId))
            //            //&& ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing) || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending))
            //            orderby atrail.SYSTEMARRIVALDATETIME descending, m.DATETIMECREATED descending
            //            //orderby m.AVAILMENTDATE descending, m.DATETIMECREATED descending
            //            select new CamProcessedLoanViewModel
            //            {
            //                loanBookingRequestId = r.LOAN_BOOKING_REQUESTID,
            //                approvalTrailId = atrail.APPROVALTRAILID,
            //                //bookingAmountRequested = r.AMOUNT_REQUESTED,
            //                requestedAmount = r.AMOUNT_REQUESTED,
            //                //approvalStatusId = (short) m.APPROVALSTATUSID,
            //                appraisalOperationId = m.OPERATIONID,
            //                loanApplicationId = m.LOANAPPLICATIONID,
            //                loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
            //                applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
            //                applicationStatusId = m.APPLICATIONSTATUSID,
            //                customerId = d.CUSTOMERID,
            //                //customerCode = from cust in context.TBL_CUSTOMER where cust.CUSTOMERID == d.CUSTOMERID select cust.CUSTOMERCODE.FirstOrDefault( ,
            //                customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
            //                customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
            //                customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
            //                customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
            //                isRelatedParty = m.ISRELATEDPARTY,
            //                customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
            //                customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
            //                customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
            //                operationId = atrail.OPERATIONID,
            //                isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
            //                isInvestmentGrade = m.ISINVESTMENTGRADE,
            //                productClassName = d.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

            //                companyId = m.COMPANYID,
            //                branchId = m.BRANCHID,
            //                branchName = m.TBL_BRANCH.BRANCHNAME,
            //                subSectorId = d.SUBSECTORID,
            //                subSectorName = d.TBL_SUB_SECTOR.NAME,
            //                sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
            //                applicationTenor = m.APPLICATIONTENOR,
            //                effectiveDate = (DateTime)d.EFFECTIVEDATE,
            //                expiryDate = (DateTime)d.EXPIRYDATE,
            //                relationshipOfficerId = m.RELATIONSHIPOFFICERID,
            //                relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
            //                relationshipManagerId = m.RELATIONSHIPMANAGERID,
            //                relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

            //                currencyId = d.CURRENCYID,
            //                currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
            //                isLocalCurrency = company.CURRENCYID == d.CURRENCYID ? true : false,
            //                exchangeRate = d.EXCHANGERATE,
            //                loanTypeId = m.LOANAPPLICATIONTYPEID,
            //                loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            //                camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
            //                productId = d.APPROVEDPRODUCTID,
            //                productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
            //                productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
            //                productName = d.TBL_PRODUCT.PRODUCTNAME,
            //                productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
            //                productClassId = m.PRODUCTCLASSID,
            //                misCode = m.MISCODE,
            //                teamMisCode = m.TEAMMISCODE,
            //                casaAccountId = r.CASAACCOUNTID,

            //                interestRate = d.APPROVEDINTERESTRATE,
            //                submittedForAppraisal = m.SUBMITTEDFORAPPRAISAL,
            //                approvedAmount = d.APPROVEDAMOUNT,
            //                approvedDate = m.APPROVEDDATE,
            //                groupApprovedAmount = m.APPROVEDAMOUNT,
            //                approvedTenor = d.APPROVEDTENOR,
            //                createdBy = m.CREATEDBY,
            //                newApplicationDate = m.APPLICATIONDATE,
            //                dateTimeCreated = d.DATETIMECREATED,
            //                availmentDate = m.AVAILMENTDATE,
            //                systemCurrentDate = systemDate,
            //                isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
            //                loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0,
            //                approvalStatusId = (short)atrail.APPROVALSTATUSID,
            //                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),

            //            }).ToList();

            // data = data.Union(data2).ToList();
            var data = data2;
            foreach (var item in data)
            {
                var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                switch (item.productTypeId)
                {
                    case (short)LoanProductTypeEnum.TermLoan:
                        decimal utilizedAmount = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) utilizedAmount = utilizedAmount + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - utilizedAmount;
                        break;
                    case (short)LoanProductTypeEnum.CommercialLoan:
                        decimal utilizedAmount2 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) utilizedAmount2 = utilizedAmount2 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - utilizedAmount2;
                        break;
                    case (short)LoanProductTypeEnum.SelfLiquidating:
                        decimal utilizedAmount3 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) utilizedAmount3 = utilizedAmount3 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - utilizedAmount3;
                        break;
                    case (short)LoanProductTypeEnum.RevolvingLoan:
                        decimal overdraftBal = 0;
                        foreach (var overdraft in overdrafts)
                        {
                            if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                        break;
                    case (short)LoanProductTypeEnum.ContingentLiability:
                        decimal contingentBal = 0;
                        foreach (var contingent in contingents)
                        {
                            if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - contingentBal;
                        break;
                    case (short)LoanProductTypeEnum.SyndicatedTermLoan:
                        decimal customerAvailableAmount4 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) utilizedAmount3 = customerAvailableAmount4 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount4;
                        break;
                }

                if (item.effectiveDate == null) item.effectiveDate = item.availmentDate;
                if (item.expiryDate == null && item.effectiveDate != null) item.expiryDate = item.effectiveDate.Value.AddDays(item.approvedTenor);

                var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                    item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED);

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                    item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                if (item.disapprovedCount > 0)
                    item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED);

                //item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);

                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                if (disbursedLoan.Any())
                {
                    item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                }
            }
            //======================================================

            //data = (from a in data where ((a.customerAvailableAmount > 0) || (a.customerAvailableAmount == null)) select a).ToList();

            //foreach (var item in data)
            //{

            //    var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);

            //    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
            //        item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED);

            //    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
            //        item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

            //    if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
            //        item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

            //    item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

            //    if (item.disapprovedCount > 0)
            //        item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED);

            //    item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);

            //    var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
            //    if (disbursedLoan.Any())
            //    {
            //        item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
            //    }

            //}

            return data;
        }

    

        private decimal getDisbursableAmount(int operationId, int loanApplicationDetailId)
        {
            var appDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            var approvedAmount = appDetail.APPROVEDAMOUNT;
            var products = context.TBL_PRODUCT.Find(appDetail.APPROVEDPRODUCTID);
            decimal? disbursableAmount = 0;
            decimal? releasedAmount = 0;

            if (products.ISFACILITYLINE == true || appDetail.ISLINEFACILITY == true)
            {
                releasedAmount = (from l in context.TBL_LOAN
                                  where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                  select (decimal?)l.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

                var summedPrincipalContingent = (from l in context.TBL_LOAN_CONTINGENT
                                                 where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                 && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                 select (decimal?)l.CONTINGENTAMOUNT).Sum() ?? 0;

                var summedPrincipalRevolving = (from l in context.TBL_LOAN_REVOLVING
                                                where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                select (decimal?)l.OVERDRAFTLIMIT).Sum() ?? 0;

                disbursableAmount = approvedAmount - (summedPrincipalRevolving + summedPrincipalContingent + releasedAmount);
            }
            else
            {
                if (operationId == (short)OperationsEnum.TermLoanBooking || operationId == (short)OperationsEnum.ForeignExchangeLoanBooking || operationId == (short)OperationsEnum.CommercialLoanBooking)
                {
                    var summedPrincipal = (from l in context.TBL_LOAN
                                           where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                           select (decimal?)l.PRINCIPALAMOUNT).Sum() ?? 0;

                    disbursableAmount = approvedAmount - summedPrincipal;
                }

                if (operationId == (short)OperationsEnum.ContigentLoanBooking)
                {
                    var summedPrincipal = (from l in context.TBL_LOAN_CONTINGENT
                                                            where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                            && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                            select (decimal?)l.CONTINGENTAMOUNT).Sum() ?? 0;

                    disbursableAmount = approvedAmount - summedPrincipal;
                }

                if (operationId == (short)OperationsEnum.RevolvingLoanBooking)
                {
                    var summedPrincipal = (from l in context.TBL_LOAN_REVOLVING
                                                            where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                            && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                            select (decimal?)l.OVERDRAFTLIMIT).Sum() ?? 0;

                    disbursableAmount = approvedAmount - summedPrincipal;
                }
            }
            

            return disbursableAmount ?? 0;
        }

        //private decimal GetLoanUtilizationReleasedAmount(int loanApplicationDetailId)
        //{
        //    var releasedLoanAmount = (from l in context.TBL_LOAN
        //                      where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
        //                      select (decimal?)l.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

        //    var contigentAmount = (from l in context.TBL_LOAN_CONTINGENT
        //             where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
        //             && l.LOANSTATUSID == (short)LoanStatusEnum.Active
        //             select (decimal?)l.CONTINGENTAMOUNT).Sum() ?? 0;

        //    var overdraftAmount = (from l in context.TBL_LOAN_REVOLVING
        //     where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
        //     && l.LOANSTATUSID == (short)LoanStatusEnum.Active
        //     select (decimal?)l.OVERDRAFTLIMIT).Sum() ?? 0;
        //}


        private void ValidateGlobalLimit(List<LoanBookingRequestViewModel> models, List<TBL_LOAN_APPLICATION_DETAIL> lineFacilities)
        {
            var affectedModels = models.Where(x => lineFacilities.Select(c => x.loanApplicationDetailId).Contains(x.loanApplicationDetailId));

            foreach(var request in models)
            {
                var existingLoans = context.TBL_LOAN.Where(x => x.CUSTOMERID == request.customerId && x.LOANAPPLICATIONDETAILID == request.loanApplicationDetailId).ToList();
                var existingOverdraft = context.TBL_LOAN_REVOLVING.Where(x => x.CUSTOMERID == request.customerId && x.LOANAPPLICATIONDETAILID == request.loanApplicationDetailId).ToList();
                var existingContingent = context.TBL_LOAN_CONTINGENT.Where(x => x.CUSTOMERID == request.customerId && x.LOANAPPLICATIONDETAILID == request.loanApplicationDetailId).ToList();

                var sumOfExistingLoans = existingLoans.Count > 0 ? existingLoans?.Sum(x => x.PRINCIPALAMOUNT) ?? (decimal)0 : 0;
                var sunOfExistingOverdrafts = existingOverdraft.Count > 0 ? existingOverdraft?.Sum(x => x.OVERDRAFTLIMIT) ?? (decimal)0 : 0;
                var sumOfExistingLiabilities = existingContingent.Count > 0 ? existingContingent?.Sum(x => x.CONTINGENTAMOUNT) ?? (decimal)0 : 0;

                var valueTaken = sumOfExistingLoans + sunOfExistingOverdrafts + sumOfExistingLiabilities;

                var currentFacility = lineFacilities.FirstOrDefault(x=>x.LOANAPPLICATIONDETAILID == request.loanApplicationDetailId);
                decimal individualGlobalLimit = currentFacility.APPROVEDLINELIMIT ?? 0;
         
                var customerRecord = context.TBL_CUSTOMER.Where(x=>x.CUSTOMERID == request.customerId || x.CUSTOMERID == currentFacility.CUSTOMERID).FirstOrDefault();
                var customer = customerRecord?.FIRSTNAME + " " + customerRecord?.LASTNAME;

                if(valueTaken >= individualGlobalLimit && individualGlobalLimit > 0) { throw new ConditionNotMetException($"The Global Limit for customer '{customer}' ({customerRecord?.CUSTOMERCODE}) has already been met."); }

                if(request.amount_Requested > lineFacilities.Sum(x => x.APPROVEDAMOUNT) ) { throw new ConditionNotMetException($"The request amount is greater than the approved amount."); }

                if(context.TBL_LOAN_BOOKING_REQUEST.Where(x=>x.LOANAPPLICATIONDETAILID == request.loanApplicationDetailId && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Any())
                {
                    if (individualGlobalLimit > 0 && request.amount_Requested > individualGlobalLimit) { throw new ConditionNotMetException($"The Global Limit for customer '{customer}' ({customerRecord?.CUSTOMERCODE}) will be exceeded.");  }
                }

                if (individualGlobalLimit > 0 && ((valueTaken + request.amount_Requested) > currentFacility.APPROVEDAMOUNT)) { throw new ConditionNotMetException($"The Global Limit for customer '{customer}' ({customerRecord?.CUSTOMERCODE}) will be exceeded. {valueTaken} already taken by customer."); }
            }
        }

        public WorkflowResponse AddLoanBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> models)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                var loanApplicationDetailIds = models.Select(p => p.loanApplicationDetailId).ToList();
                var lineFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.ISLINEFACILITY == true && loanApplicationDetailIds.Contains(x.LOANAPPLICATIONDETAILID)).ToList();
                if(lineFacilities.Count > 0) { ValidateGlobalLimit(models, lineFacilities); }

                foreach (var model in models)
                {
                    if (model.approvalStatusId != (short)ApprovalStatusEnum.Referred)
                    {
                        if (!AddLoanBookingRequests(applicationStatusId, model))
                        {
                            //if (model.isLienPlacementForLoan)
                            //{
                            //    var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
                            //    {
                            //        username = model.username,
                            //        passcode = model.passCode
                            //    };
                            //    PlaceLien(model.loanApplicationDetailId, twoFactorAuthDetails);
                            //}
                            trans.Rollback();
                            workflow.Response = null;
                            return workflow.Response;
                            //return false;
                        }
                    }
                    else
                    {
                        if (!UpdateLoanBookingRequests(applicationStatusId, model))
                        {
                            trans.Rollback();
                            workflow.Response = null;
                            return workflow.Response;
                            //return false;
                        }
                    }

                    //if (model.isLienPlacementForLoan)
                    //{
                    //    var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
                    //    {
                    //        username = model.username,
                    //        passcode = model.passCode
                    //    };

                    //    PlaceLienForLoan(model, twoFactorAuthDetails);
                    //}
                }
                trans.Commit();
                return workflow.Response;
                //return true;
            }
        }

        public int GetNextLevelForBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> entities)
        {
            var entity = entities.FirstOrDefault();
            using (var trans = context.Database.BeginTransaction())
            {
                var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

                var requestedFacility = context.TBL_PRODUCT.Find(entity.productId);

                //var request = new TBL_LOAN_BOOKING_REQUEST
                //{
                //    AMOUNT_REQUESTED = entity.amount_Requested,
                //    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                //    LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                //    CASAACCOUNTID = entity.casaAccountId,
                //    CUSTOMERID = entity.customerId,
                //    CASAACCOUNTID2 = entity.casaAccountId2,
                //    ISUSED = false,
                //    PRODUCTID = entity.productId,
                //    DATETIMECREATED = DateTime.Now,
                //    CREATEDBY = entity.createdBy,
                //    TENOR = entity.tenor,
                //    TAKEFEEONCE = entity.chargeFeeOnce,
                //};
                //context.TBL_LOAN_BOOKING_REQUEST.Add(request);
                //context.SaveChanges();

                var approvalModel = new ForwardViewModel
                {
                    createdBy = entity.createdBy,
                    companyId = entity.companyId,
                    applicationId = 1,
                    comment = entity.comment,
                    //comment = "Please approve this request for loan booking",
                    amount = entity.amount_Requested,
                };

                if (requestedFacility.PRODUCTCLASSID == (short)ProductClassEnum.Creditcards)
                {
                    approvalModel.operationId = (short)OperationsEnum.CreditCardDrawdownRequest;
                    approvalModel.forwardAction = (int)ApprovalStatusEnum.Pending;
                    LogApprovalForMessage(approvalModel, true);
                }
                else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                {
                    if (requestedFacility.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased)
                    {
                        approvalModel.operationId = (short)OperationsEnum.CorporateDrawdownRequest;
                        approvalModel.forwardAction = (int)ApprovalStatusEnum.Pending;
                        LogApprovalForMessage(approvalModel, true);
                    }
                    else
                    {
                        approvalModel.operationId = (short)OperationsEnum.IndividualDrawdownRequest;
                        approvalModel.forwardAction = (int)ApprovalStatusEnum.Pending;
                        LogApprovalForMessage(approvalModel, true);
                    }
                }
                else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                {
                    if (GetRevolvingTrancheDisbursementOperationId(loanApplicationDetails.LOANAPPLICATIONDETAILID))
                    {
                        approvalModel.operationId = (short)OperationsEnum.RevolvingTranchDisbursement;
                        approvalModel.forwardAction = (int)ApprovalStatusEnum.Pending;
                        LogApprovalForMessage(approvalModel, true);
                    }
                    else
                    {
                        approvalModel.operationId = (short)OperationsEnum.CorporateDrawdownRequest;
                        approvalModel.forwardAction = (int)ApprovalStatusEnum.Pending;
                        LogApprovalForMessage(approvalModel, true);
                    }
                }
                trans.Rollback();
                return workflow.Response.nextLevelId.Value;
            }
        }

        private bool AddLoanBookingRequests(int applicationStatusId, LoanBookingRequestViewModel entity)
        {
            List<LoanBookingRequestViewModel> models = new List<LoanBookingRequestViewModel>();
            models.Add(entity);

            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);
            var loanApplicationDetailIds = models.Select(p => p.loanApplicationDetailId).ToList();
            var lineFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.ISLINEFACILITY == true && loanApplicationDetailIds.Contains(x.LOANAPPLICATIONDETAILID)).ToList();
            if (lineFacilities.Count > 0) { ValidateGlobalLimit(models, lineFacilities); }

            if (entity.amount_Requested > loanApplicationDetails.APPROVEDAMOUNT)
            {
                throw new ConditionNotMetException("Requested Amount cannot be greater than the approved amount");
            }

            if (entity.customerId == null)
            {
                if (loanApplicationDetails.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.CustomerGroup)
                {
                    throw new SecureException("Please select a Group Member");
                }
                else
                {
                    entity.customerId = loanApplicationDetails.CUSTOMERID;
                }
            }

            if (context.TBL_LOAN_BOOKING_REQUEST.Any(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId && x.CUSTOMERID == entity.customerId && !(x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && x.ISUSED == true) && (x.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved) && x.DELETED == false))
            {
                var loanReq = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId && x.CUSTOMERID == entity.customerId && 
                !(x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && x.ISUSED == true) && (x.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved) && x.DELETED == false).FirstOrDefault();
                if (entity.productId == 156)
                {
                    context.TBL_LOAN_BOOKING_REQUEST.Remove(loanReq);
                }
                else
                {
                    throw new ConditionNotMetException("This facility already has a tranche disbursement request for this customer currently undergoing approval.");
                }
                
            }

            //if ((loanApplicationDetails.ISLINEFACILITY ?? false))
            //{
            //    var bookingRequestsForCustomer = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId && x.CUSTOMERID == entity.customerId && x.DELETED == false).ToList();
            //    var totalAmountRequested = bookingRequestsForCustomer.Sum(r => r.AMOUNT_REQUESTED);
            //    if ((entity.amount_Requested + totalAmountRequested) > (loanApplicationDetails.APPROVEDLINELIMIT ?? 0))
            //    {
            //        throw new ConditionNotMetException("Requested Amount(s) for this customer cannot be greater than the approved line limit");
            //    }
            //}

            //if (entity.tenor > loanApplicationDetails.APPROVEDTENOR)
            //{
            //    throw new ConditionNotMetException("Requested Tenor cannot be greater than the approved tenor");
            //}

            var requestedFacility = context.TBL_PRODUCT.Find(entity.productId);
            var operationId = 0;
            var productTypeId = requestedFacility.PRODUCTTYPEID;

            if (productTypeId == (short)LoanProductTypeEnum.TermLoan || productTypeId == (short)LoanProductTypeEnum.SelfLiquidating || productTypeId == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                operationId = (short)OperationsEnum.TermLoanBooking;

            if (productTypeId == (short)LoanProductTypeEnum.CommercialLoan)
                operationId = (short)OperationsEnum.CommercialLoanBooking;

            if (productTypeId == (short)LoanProductTypeEnum.RevolvingLoan)
                operationId = (short)OperationsEnum.RevolvingLoanBooking;

            if (productTypeId == (short)LoanProductTypeEnum.ForeignXRevolving)
                operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;

            if (productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                operationId = (short)OperationsEnum.ContigentLoanBooking;

            if (entity.amount_Requested > getDisbursableAmount(operationId, entity.loanApplicationDetailId))
            {
                throw new ConditionNotMetException("Requested Amount cannot be greater than the disbursable amount");
            }

            bool cleared = OfferLetterChecklistValidation(loanApplicationDetails.LOANAPPLICATIONID, 1);
            if (cleared == false) throw new SecureException("Checklist not cleared to go further!");
            
            if (entity.casaAccountId2 == 0) entity.casaAccountId2 = null;
            if (entity.casaAccountId == 0) entity.casaAccountId = null;
            var request = new TBL_LOAN_BOOKING_REQUEST
            {
                AMOUNT_REQUESTED = entity.amount_Requested,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                CASAACCOUNTID = entity.casaAccountId,
                CUSTOMERID = entity.customerId,
                CASAACCOUNTID2 = entity.casaAccountId2,
                ISUSED = false,
                PRODUCTID = entity.productId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = entity.createdBy,
                TENOR = entity.tenor,
                TAKEFEEONCE = entity.chargeFeeOnce,
            };
            context.TBL_LOAN_BOOKING_REQUEST.Add(request);
            context.SaveChanges();

            var approvalModel = new ForwardViewModel
            {
                createdBy = entity.createdBy,
                companyId = entity.companyId,
                applicationId = request.LOAN_BOOKING_REQUESTID,
                comment = entity.comment,
                //comment = "Please approve this request for loan booking",
                amount = entity.amount_Requested,
                toStaffId = entity.toStaffId,
            };
            if (entity.productId == 156 || entity.productId == 228 || entity.productId == 297 || entity.productId == 354)
            {
                    LogApproval(approvalModel, (short)OperationsEnum.IBLAvailmentInProgress, true, (int)ApprovalStatusEnum.Pending);
                    request.OPERATIONID = (short)OperationsEnum.IBLAvailmentInProgress;
                 }
                else if (requestedFacility.PRODUCTCLASSID == (short)ProductClassEnum.Creditcards)
                {
                    LogApproval(approvalModel, (short)OperationsEnum.CreditCardDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                    request.OPERATIONID = (short)OperationsEnum.CreditCardDrawdownRequest;
                }
                else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                {
                    if (requestedFacility.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased || requestedFacility.TBL_PRODUCT_CLASS.PRODUCTCLASSID == (short)ProductClassEnum.MortgageLoan)
                    {
                        LogApproval(approvalModel, (short)OperationsEnum.CorporateDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                        request.OPERATIONID = (short)OperationsEnum.CorporateDrawdownRequest;
                    }
                    else
                    {
                        LogApproval(approvalModel, (short)OperationsEnum.IndividualDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                        request.OPERATIONID = (short)OperationsEnum.IndividualDrawdownRequest;
                    }
                }
                else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Corporate)
                {
                    if (GetRevolvingTrancheDisbursementOperationId(loanApplicationDetails.LOANAPPLICATIONDETAILID))
                    {
                        LogApproval(approvalModel, (short)OperationsEnum.RevolvingTranchDisbursement, true, (int)ApprovalStatusEnum.Pending);
                        request.OPERATIONID = (short)OperationsEnum.RevolvingTranchDisbursement;
                    }
                    else
                    {
                        LogApproval(approvalModel, (short)OperationsEnum.CorporateDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                        request.OPERATIONID = (short)OperationsEnum.CorporateDrawdownRequest;
                    }
                }
            
            


            if (entity.chargeFeeOnce == true) { loanApplicationDetails.TAKEFEETYPEID = (short)TakeFeeTypeEnum.ApprovedAmount; }
            else loanApplicationDetails.TAKEFEETYPEID = (short)TakeFeeTypeEnum.UtilisedAmount;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingRequested,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                TARGETID = request.LOAN_BOOKING_REQUESTID,
                DETAIL = $"Request to book loan of amount '{ entity.amount_Requested }' for customer id'{loanApplicationDetails.TBL_CUSTOMER.CUSTOMERCODE}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            context.TBL_AUDIT.Add(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;

        }

        private bool OfferLetterChecklistValidation(int id, int type)
        {
            int count = 0;
            if (type == 1)
            {
                var detailids = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == id)
                    .Select(x => x.LOANAPPLICATIONDETAILID)
                    .ToList();

                count = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => detailids.Contains(x.LOANAPPLICATIONDETAILID)
                        && x.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred
                        && x.ISSUBSEQUENT == false
                    )
                    .Count();
            }

            return count == 0;
        }

        private TBL_LOAN_BOOKING_REQUEST addBookingRequest(multipleDisbursementOutputViewModel entity, short? approvalStatusid, UserInfo user)
        {
            var request = new TBL_LOAN_BOOKING_REQUEST
            {
                AMOUNT_REQUESTED = entity.loanAmount,
                APPROVALSTATUSID = approvalStatusid == null ? (short)ApprovalStatusEnum.Pending : (short)approvalStatusid,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                CASAACCOUNTID = entity.casaAccountId,
                CASAACCOUNTID2 = entity.casaAccountId2,
                ISUSED = approvalStatusid == (short)ApprovalStatusEnum.Approved ? true : false,
                PRODUCTID = (short)entity.productId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = user.createdBy,
            };
            context.TBL_LOAN_BOOKING_REQUEST.Add(request);
            context.SaveChanges();
            return request;
        }

        private void PlaceLien(int loanApplicationDetailId, TwoFactorAutheticationViewModel twoFactorAuthDetails, int createdBy)
        {
            //TODO fetch the AccountBalance for account from the API

            var app = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);

            List<int> collateralTypesIds = new List<int>();
            collateralTypesIds.Add((int)CollateralTypeEnum.CASA);
            collateralTypesIds.Add((int)CollateralTypeEnum.FixedDeposit);
            //collateralTypesIds.Add((int)CollateralTypeEnum.DomiciliationContract);
            collateralTypesIds.Add((int)CollateralTypeEnum.TreasuryBillsAndBonds);

            //var collateralMappings = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId && x.DELETED == false).ToList();
            //var mappedCollateralIds = collateralMappings.Select(m => m.COLLATERALCUSTOMERID).ToList();
            //var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => mappedCollateralIds.Contains(x.COLLATERALCUSTOMERID) && collateralTypesIds.Contains(x.COLLATERALTYPEID));
            var loanLienDetail = context.TBL_APPLICATIONDETAIL_LIEN.Where(l => l.APPLICATIONDETAILID == loanApplicationDetailId && l.DELETED == false && l.ISRELEASED == false).ToList();
            var mappedCollateralIds = loanLienDetail.Select(m => m.COLLATERALCUSTOMERID).ToList();
            var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => mappedCollateralIds.Contains(x.COLLATERALCUSTOMERID) && collateralTypesIds.Contains(x.COLLATERALTYPEID));

            foreach (var item in collaterals)
            {
                TBL_CASA casa = new TBL_CASA();
                if (item.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                {
                    var accountNumber = item.TBL_COLLATERAL_CASA.FirstOrDefault().ACCOUNTNUMBER;
                    casa = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == accountNumber).FirstOrDefault();
                }

                var casaBalance = integration.GetCustomerAccountBalance(casa?.PRODUCTACCOUNTNUMBER);

                var staffCode = context.TBL_STAFF.Where(O => O.STAFFID == createdBy).FirstOrDefault().STAFFCODE;

                var lienModel = new FlexcubeLienViewModel
                {
                    account_no = casa?.PRODUCTACCOUNTNUMBER,
                    collateral_code = item.COLLATERALCODE,
                    collateral_value = item.COLLATERALVALUE.ToString(),
                    start_date = app.EFFECTIVEDATE.Value.ToString("ddMMMyyyy"),
                    end_date = app.EFFECTIVEDATE.Value.AddDays(app.APPROVEDTENOR).ToString("ddMMMyyyy"),
                    collateral_id = item.COLLATERALCUSTOMERID.ToString(),
                    contract_ref_no = app.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    collateral_contribution = casaBalance?.availableBalance.ToString(),
                    branch_code = app.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHCODE,
                    channel_code = "FINTRAK",
                    maker_id = staffCode,
                    checker_id = staffCode,
                    loanApplicationId = app.LOANAPPLICATIONID
                };

                integration.FlexcubeCasaLien(lienModel, twoFactorAuthDetails);
            }


        }

        private bool UpdateLoanBookingRequests(int applicationStatusId, LoanBookingRequestViewModel model)
        {
            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(model.loanBookingRequestId);

            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(request.LOANAPPLICATIONDETAILID);
            if (model.amount_Requested > loanApplicationDetails.APPROVEDAMOUNT)
            {
                throw new ConditionNotMetException("Requested Amount cannot be greater than the approved amount");
            }

            if (request != null)
            {
                model.approvalStatusId = (short)ApprovalStatusEnum.Approved;

                request.AMOUNT_REQUESTED = model.amount_Requested;
                request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                //request.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                request.CASAACCOUNTID = model.casaAccountId;
                request.CASAACCOUNTID2 = model.casaAccountId2;
                request.ISUSED = false;
                request.PRODUCTID = model.productId;
                request.DATETIMEUPDATED = DateTime.Now;
                request.LASTUPDATEDBY = model.createdBy;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                //workflow.StatusId = ((int)model.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)model.approvalStatusId;
                workflow.TargetId = request.LOAN_BOOKING_REQUESTID;
                workflow.Comment = model.comment != null ? model.comment : "Kindly proceeed. Update has been applied";
                workflow.OperationId = model.operationId ?? 0;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = false;

                workflow.LogActivity();

                //return false;
                return context.SaveChanges() > 0;
            }
            return false;
        }


        private IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int companyId, int staffId, int targetId, int operationId)
        {
            var loggedsStaff = context.TBL_STAFF.Find(staffId);
            var result = (from a in context.TBL_APPROVAL_TRAIL
                              // join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                              //  join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                              // join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                              // join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                              // join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                              //join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                              // from j in apprStaff.DefaultIfEmpty()
                              // join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                          where a.COMPANYID == companyId
                          where a.TARGETID == targetId && a.OPERATIONID == operationId
                          select new WorkflowTrackerViewModel
                          {
                              arrivalDate = a.ARRIVALDATE,
                              responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? a.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                              responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                              systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                              systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                              responseStaffName = !a.TOAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                              comment = a.COMMENT,
                              requestStaffName = a.TBL_STAFF.FIRSTNAME != null ? a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME : null,
                              requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                              TargetId = a.TARGETID,
                              // operationId = e.OPERATIONID,
                              // operationName = e.OPERATIONNAME,
                              //approvalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID == a.APPROVALSTATUSID).FirstOrDefault().APPROVALSTATUSNAME
                              approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME
                          }).Distinct();


            var response = result.ToList();
            return result;
        }


        //public async Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId)
        //{
        //    var result = await GetApprovalTrail(companyId, staffId, targetId, operationId).ToListAsync();
        //    return result.Distinct();
        //}
    }
}