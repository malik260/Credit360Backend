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

namespace FintrakBanking.Repositories.Credit
{
    public class CreditDrawdownRepository : ICreditDrawdownRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;
        private IAuditTrailRepository audit;
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

        public int GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.targetId);
                var applicationDet = context.TBL_LOAN_APPLICATION_DETAIL.Find(request.LOANAPPLICATIONDETAILID);
                var application = context.TBL_LOAN_APPLICATION.Find(applicationDet.LOANAPPLICATIONID);

                // checking of company limit at availment
                var exposure = GetCurrentCompanyExposure();
                var proposedExposure = exposure.proposedLimit + applicationDet.APPROVEDAMOUNT;
                var company = context.TBL_COMPANY.Find(application.COMPANYID);
                if (proposedExposure >= company.SHAREHOLDERSFUND)
                {
                    throw new SecureException("Company Limit Exceeded!");
                }

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                workflow.TargetId = entity.targetId;
                workflow.Comment = entity.comment;
                workflow.OperationId = entity.operationId;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = false;
                workflow.Amount = request.AMOUNT_REQUESTED;
                workflow.BusinessUnitId = applicationDet.TBL_CUSTOMER?.BUSINESSUNTID;
                //if(request.AMOUNT_REQUESTED > 100000000)
                //workflow.FinalLevel = application.TRANCHEAPPROVAL_LEVELID;

                //if (GetCurrentApprovalLevelId(entity.companyId, entity.operationId, entity.targetId) == application.TRANCHEAPPROVAL_LEVELID)
                //{
                //    workflow.NextLevelId = GetFirstAvailmentLevelId(entity.operationId);
                //}

                workflow.LevelBusinessRule = new LevelBusinessRule
                {
                    Amount = request.AMOUNT_REQUESTED,
                    PepAmount = request.AMOUNT_REQUESTED,
                    Pep = application.ISPOLITICALLYEXPOSED,
                    InsiderRelated = application.ISRELATEDPARTY,
                    ProjectRelated = application.ISPROJECTRELATED,
                    OnLending = application.ISONLENDING,
                    InterventionFunds = application.ISINTERVENTIONFUNDS,
                    OrrBasedApproval = application.ISORRBASEDAPPROVAL,
                    DomiciliationNotInPlace = application.DOMICILIATIONNOTINPLACE,
                };

                workflow.LogActivity();

                context.SaveChanges();

                if (entity.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                    trans.Commit();
                    context.SaveChanges();
                    return 3;
                }

                else if (workflow.NewState == (int)ApprovalState.Ended)
                {
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    var operationId = 0;
                    if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan)
                        operationId = (short)OperationsEnum.CommercialLoanBooking;
                    if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                        operationId = (short)OperationsEnum.ContigentLoanBooking;
                    if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                        operationId = (short)OperationsEnum.TermLoanBooking;
                    if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving)
                        operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                    if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                        operationId = (short)OperationsEnum.RevolvingLoanBooking;

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = request.LOAN_BOOKING_REQUESTID,
                        comment = "A request for booking needs your attention",
                        amount = request.AMOUNT_REQUESTED,
                    };

                    if (operationId > 0) LogApproval(approvalModel, operationId, true, (short)ApprovalStatusEnum.Pending);
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                    var loanLienDetail = context.TBL_APPLICATIONDETAIL_LIEN.FirstOrDefault(l => l.APPLICATIONDETAILID == request.LOANAPPLICATIONDETAILID && l.DELETED == false && l.ISRELEASED == false);
                    if (loanLienDetail != null)
                    {
                        var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
                        {
                            username = "model.username",//for test, real value to be passed!!!
                            passcode = "model.passCode"
                        };
                        PlaceLien(loanLienDetail.APPLICATIONDETAILID, twoFactorAuthDetails, entity.createdBy);
                    }

                    //PlaceLien(request.LOANAPPLICATIONDETAILID, new TwoFactorAutheticationViewModel() { username = "model.username", passcode = "model.passCode" });

                    context.SaveChanges();
                    trans.Commit();
                    return 0;
                }

                else
                {
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestInitiated;
                    context.SaveChanges();
                    trans.Commit();
                    return 1;
                }
            }

            //return workflow.Response;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId, bool isInitiation = false)
        {
            List<int> operationIds = new List<int>();
            operationIds.Add((int)OperationsEnum.CorporateDrawdownRequest);
            operationIds.Add((int)OperationsEnum.IndividualDrawdownRequest);
            operationIds.Add((int)OperationsEnum.CreditCardDrawdownRequest);
            var staffs = generalSetup.GetStaffRlieved(staffId);

            List<int> levelIds = new List<int>();
            foreach (var i in operationIds) { levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, i).ToList()); }

            //levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CorporateDrawdownRequest).ToList());
            //levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IndividualDrawdownRequest).ToList());
            //levelIds.AddRange(generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CreditCardDrawdownRequest).ToList());

            List<CamProcessedLoanViewModel> data = new List<CamProcessedLoanViewModel>();

            data = (from req in context.TBL_LOAN_BOOKING_REQUEST
                    join d in context.TBL_LOAN_APPLICATION_DETAIL on req.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                    join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                    join coy in context.TBL_COMPANY on m.COMPANYID equals coy.COMPANYID
                    join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                    join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                    join br in context.TBL_BRANCH on m.BRANCHID equals br.BRANCHID
                    join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                    where operationIds.Contains(atrail.OPERATIONID)
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress
                            && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                            && req.ISUSED == false && atrail.RESPONSESTAFFID == null
                            && ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                            || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                            || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred))
                            && (req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                            && ( (levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null) 
                              || (!levelIds.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == staffId))
                          //|| (isInitiation == true && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved && req.DELETED == false)

                    orderby d.LOANAPPLICATIONDETAILID descending

                    select new CamProcessedLoanViewModel
                    {
                        loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                        approvalTrailId = atrail.APPROVALTRAILID,
                        approvalStatusId = (short)atrail.APPROVALSTATUSID,
                        approvalStatusName = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                        loanApplicationId = m.LOANAPPLICATIONID,
                        loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                        applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                        applicationStatusId = m.APPLICATIONSTATUSID,
                        appraisalOperationId = m.OPERATIONID,
                        operationId = atrail.OPERATIONID,
                        requestedAmount = req.AMOUNT_REQUESTED,
                        customerId = d.CUSTOMERID,
                        customerCode = cust.CUSTOMERCODE,
                        systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                        customerName = cust.FIRSTNAME + " " + cust.MIDDLENAME + " " + cust.LASTNAME,
                        customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                        customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                        isRelatedParty = m.ISRELATEDPARTY,
                        customerSensitivityLevelId = cust.CUSTOMERSENSITIVITYLEVELID,
                        customerOccupation = cust.OCCUPATION,
                        customerType = cust.TBL_CUSTOMER_TYPE.NAME,
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
                        productId = d.APPROVEDPRODUCTID,
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
                        createdBy = m.CREATEDBY,
                        newApplicationDate = m.APPLICATIONDATE,
                        dateTimeCreated = d.DATETIMECREATED,
                        availmentDate = m.AVAILMENTDATE,
                        requestDate = req.DATETIMECREATED,
                        //staffId = (atrail.LOOPEDSTAFFID != null ? atrail.LOOPEDSTAFFID : atrail.TOSTAFFID),
                        divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == d.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                    }).ToList();

            data = data.Where(x => x.applicationReferenceNumber != "-")
              .GroupBy(p => p.loanBookingRequestId)
              .Select(g => g.First())
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
                operationId = (short)OperationsEnum.CorporateDrawdownRequest;
            }

            return operationId;
        }

        private IEnumerable<CamProcessedLoanViewModel> AvailedLoanApplicationsDetails(int companyId, int staffId, int branchId)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);

            var data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                         where a.COMPANYID == companyId && d.DELETED == false
                         && a.CREATEDBY == staffId
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
                             createdBy = a.CREATEDBY,
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
                            createdBy = m.CREATEDBY,
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

        public IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);

            var data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                         where a.COMPANYID == companyId && d.DELETED == false
                         && a.CREATEDBY == staffId
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
                             appraisalOperationId = a.OPERATIONID,
                             requestedAmount = 0,
                             loanApplicationId = a.LOANAPPLICATIONID,
                             loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             customerId = d.CUSTOMERID,
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
                             createdBy = a.CREATEDBY,
                             newApplicationDate = a.APPLICATIONDATE,
                             dateTimeCreated = d.DATETIMECREATED,
                             availmentDate = a.AVAILMENTDATE,
                             systemCurrentDate = systemDate,
                             isTemporaryOverdraft = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == d.PROPOSEDPRODUCTID && x.ISTEMPORARYOVERDRAFT == true).Any(),
                             loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID ?? 0,

                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                         }).ToList();

            var data = data2;
           

            //var referredItem = GetBookingRequestAwaitingApproval(staffId, companyId, true).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Referred).ToList();
            // data.AddRange(referredItem);

            foreach (var item in data)
            {
                var approvedLCIssuanceIds = context.TBL_LC_ISSUANCE.Where(t => t.APPLICATIONSTATUSID != null && t.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LcIssuanceInProgress).Select(t => t.LCISSUANCEID).ToList();
                var lcIFFRequests = context.TBL_LC_ISSUANCE.Where(l => l.DELETED == false && l.FUNDSOURCEID == (int)LCFundSource.IFF);
                var lcapprovedLCIFFs = lcIFFRequests.Where(i => approvedLCIssuanceIds.Contains(i.LCISSUANCEID)).Select(i => new { i.FUNDSOURCEDETAILS, i.LETTEROFCREDITAMOUNT });
                var lcapprovedLCIFFsRecords = lcapprovedLCIFFs.Where(i => i.FUNDSOURCEDETAILS == item.loanApplicationId);
                var lcApprovedAmounts = lcapprovedLCIFFsRecords.Count() > 0 ? lcapprovedLCIFFsRecords?.Sum(i => i.LETTEROFCREDITAMOUNT) : 0;

                var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && r.DELETED == false);
                //item.operationId = GetDrawdownOperationId(item.loanApplicationDetailId);
                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                { item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED); }

                if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                //{ item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
                { item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED); }

                if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                //{ item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount; }
                { item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED); }

                item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                if (item.disapprovedCount > 0)
                { item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED); }

                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
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

        public IEnumerable<CamProcessedLoanViewModel> getApplicationsToBeAdhocApprovedForInitiateBooking(int companyId, int staffId, int branchId)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var company = context.TBL_COMPANY.Find(companyId);

            //IEnumerable<CamProcessedLoanViewModel> data2;

            var data2 = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                         where a.COMPANYID == companyId && d.DELETED == false
                         && a.CREATEDBY == staffId
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
                             createdBy = a.CREATEDBY,
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
            decimal? disbursableAmount = 0;
            if (operationId == (short)OperationsEnum.TermLoanBooking
                || operationId == (short)OperationsEnum.ForeignExchangeLoanBooking
                || operationId == (short)OperationsEnum.CommercialLoanBooking)
            {
                var summedPrincipal = (from l in context.TBL_LOAN
                                       where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                       select (decimal?)l.PRINCIPALAMOUNT).Sum() ?? 0;
                disbursableAmount = approvedAmount - summedPrincipal;
            }
            if (operationId == (short)OperationsEnum.ContigentLoanBooking)
            {
                var summedPrincipal = approvedAmount - (from l in context.TBL_LOAN_CONTINGENT
                                                        where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                        select (decimal?)l.CONTINGENTAMOUNT).Sum() ?? 0;
                disbursableAmount = approvedAmount - summedPrincipal;
            }
            if (operationId == (short)OperationsEnum.RevolvingLoanBooking)
            {
                var summedPrincipal = approvedAmount - (from l in context.TBL_LOAN_REVOLVING
                                                        where l.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                                        select (decimal?)l.OVERDRAFTLIMIT).Sum() ?? 0;
                disbursableAmount = approvedAmount - summedPrincipal;
            }
            return disbursableAmount ?? 0;
        }

        public bool AddLoanBookingRequest(int applicationStatusId, List<LoanBookingRequestViewModel> models)
        {
            using (var trans = context.Database.BeginTransaction())
            {
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
                            return false;
                        }
                    }
                    else
                    {
                        if (!UpdateLoanBookingRequests(applicationStatusId, model))
                        {
                            trans.Rollback();
                            return false;
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
                return true;
            }
        }

        private bool AddLoanBookingRequests(int applicationStatusId, LoanBookingRequestViewModel entity)
        {

            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);
            if (entity.amount_Requested > loanApplicationDetails.APPROVEDAMOUNT)
            {
                throw new ConditionNotMetException("Requested Amount cannot be greater than the approved amount");
            }

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
                CASAACCOUNTID2 = entity.casaAccountId2,
                ISUSED = false,
                PRODUCTID = entity.productId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = entity.createdBy,
                TENOR = entity.tenor,

            };
            context.TBL_LOAN_BOOKING_REQUEST.Add(request);
            context.SaveChanges();

            var approvalModel = new ForwardViewModel
            {
                createdBy = entity.createdBy,
                companyId = entity.companyId,
                applicationId = request.LOAN_BOOKING_REQUESTID,
                comment = "Please approve this request for loan booking",
                amount = entity.amount_Requested,
            };

            if (requestedFacility.PRODUCTCLASSID == (short)ProductClassEnum.Creditcards)
            {
                LogApproval(approvalModel, (short)OperationsEnum.CreditCardDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                request.OPERATIONID = (short)OperationsEnum.CreditCardDrawdownRequest;
            }
            else if (loanApplicationDetails.TBL_CUSTOMER.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
            {
                if (requestedFacility.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased)
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
                LogApproval(approvalModel, (short)OperationsEnum.CorporateDrawdownRequest, true, (int)ApprovalStatusEnum.Pending);
                request.OPERATIONID = (short)OperationsEnum.CorporateDrawdownRequest;
            }


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
            collateralTypesIds.Add((int)CollateralTypeEnum.TermDeposit);
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
                    casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.TBL_COLLATERAL_CASA.FirstOrDefault().COLLATERALCASAID).FirstOrDefault();
                }

                var casaBalance = integration.GetCustomerAccountBalance(casa.PRODUCTACCOUNTNUMBER);

                var staffCode = context.TBL_STAFF.Where(O => O.STAFFID == createdBy).FirstOrDefault().STAFFCODE;

                var lienModel = new FlexcubeLienViewModel
                {
                    account_no = casa.PRODUCTACCOUNTNUMBER,
                    collateral_code = item.COLLATERALCODE,
                    collateral_value = item.COLLATERALVALUE.ToString(),
                    start_date = app.EFFECTIVEDATE.Value.ToString("ddMMMyyyy"),
                    end_date = app.EFFECTIVEDATE.Value.AddDays(app.APPROVEDTENOR).ToString("ddMMMyyyy"),
                    collateral_id = item.COLLATERALCUSTOMERID.ToString(),
                    contract_ref_no = app.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    collateral_contribution = casaBalance.availableBalance.ToString(),
                    branch_code = app.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHCODE,
                    channel_code = "FINTRAK",
                    maker_id = staffCode,
                    checker_id = staffCode,
                    loanApplicationId = app.LOANAPPLICATIONID
                };

                integration.FlexcubeCasaLien(lienModel);
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