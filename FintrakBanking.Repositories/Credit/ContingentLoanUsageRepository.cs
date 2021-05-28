using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using System.Data.Entity;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Common.AlertMonitoring;
using System.Configuration;

namespace FintrakBanking.Repositories.Credit
{
    public class ContingentLoanUsageRepository : IContingentLoanUsageRepository
    {
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IWorkflow workflow;
        private IAuditTrailRepository auditTrail;
        private ICasaLienRepository casaLien;
        private FinTrakBankingDocumentsContext documentsContext;

        public string entiry { get; private set; }

        public ContingentLoanUsageRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
            IAuditTrailRepository auditTrail, IWorkflow workflow, ICasaLienRepository casaLien, FinTrakBankingDocumentsContext documentsContext)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.casaLien = casaLien;
            this.documentsContext = documentsContext;
        }

        public IEnumerable<ContingentLoansViewModel> GetAllContingentLoans(int staffId, int companyId)
        {
            try
            {
                int[] operations = {
                    (int)OperationsEnum.APS_RelaseChecklist,
                    (int)OperationsEnum.APS_ReleaseCAP,
                    (int)OperationsEnum.APS_ReleasePrincipaRequest,
                    (int)OperationsEnum.APSReleaseApproval
                };


                List<ContingentLoansViewModel> contingentData = new List<ContingentLoansViewModel>();
                DateTime currentDate = genSetup.GetApplicationDate();
                var data = (from a in context.TBL_LOAN_CONTINGENT
                            join r in context.TBL_LMSR_APPLICATION_DETAIL on a.CONTINGENTLOANID equals r.LOANID
                            join l in context.TBL_LMSR_APPLICATION on r.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                            join b in context.TBL_PRODUCT_BEHAVIOUR on a.PRODUCTID equals b.PRODUCTID
                            where l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            //&& currentDate <= a.MATURITYDATE 
                            && operations.Contains(l.OPERATIONID)
                            && r.OPERATIONPERFORMED == false
                            && r.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability

                            select new ContingentLoansViewModel()
                            {
                                principalName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                                bookingDate = a.BOOKINGDATE,
                                casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                facilityAmount = a.CONTINGENTAMOUNT,
                                contingentLoanId = a.CONTINGENTLOANID,
                                currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                                currencyId = a.CURRENCYID,
                                customerId = a.CUSTOMERID,
                                firstName = a.TBL_CUSTOMER.FIRSTNAME,
                                lastName = a.TBL_CUSTOMER.LASTNAME,
                                middleName = a.TBL_CUSTOMER.MIDDLENAME,
                                productId = a.PRODUCTID,
                                effectiveDate = a.EFFECTIVEDATE,
                                exchangeRate = a.EXCHANGERATE,
                                loanApplicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                loanReferenceNumber = a.LOANREFERENCENUMBER,
                                maturityDate = a.MATURITYDATE,
                                productName = a.TBL_PRODUCT.PRODUCTNAME,
                                loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                amountRequested = r.CUSTOMERPROPOSEDAMOUNT,
                                loanReviewApplicationId = r.LOANREVIEWAPPLICATIONID,
                                operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == l.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                loanApplicationNumber=l.APPLICATIONREFERENCENUMBER,


                            }).ToList();


                foreach (var item in data)
                {
                    var usedData = context.TBL_LOAN_CONTINGENT_USAGE.Where(d => d.CONTINGENTLOANID == item.contingentLoanId);
                    if (usedData.Any())
                    {
                        item.usedAmount = usedData.Sum(c => c.AMOUNTREQUESTED);
                        // item.amountRemaining = item.facilityAmount - item.usedAmount;
                    }
                    contingentData.Add(item);

                }

                var dat = contingentData.ToList();
                // var dat = contingentData.Where(c => c.percentageUsed < 100).OrderByDescending(d => d.contingentLoanId).ToList();
                return dat;
            }
            catch (Exception ex)
            {
                throw new ConditionNotMetException(ex.Message);
            }
        }

        // TO REFACOR & REMOVE
        private bool LogForApproval(ApproveAPSRequestViewModel entity)
        {
            bool response = false;
            workflow.StaffId = entity.staffId;
            workflow.OperationId = entity.operationId;
            workflow.TargetId = entity.targetId;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.Comment = entity.comment;
            workflow.ExternalInitialization = entity.externalInitialization;
            workflow.DeferredExecution = entity.deferredExecution;
            return response = workflow.LogActivity();

        }

        public bool SaveContigentLoans(ContingentLoanUsageViewModel entity)
        {

            var data = new TBL_LOAN_CONTINGENT_USAGE
            {
                AMOUNTREQUESTED = entity.amountRequested,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                CONTINGENTLOANID = entity.contingentLoanId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                REMARK = entity.remark,
                LOANREFERENCENUMBER = entity.loanReferenceNumber,
                LOANREVIEWAPPLICATIONID = entity.loanReviewApplicationId
            };
            var model = context.TBL_LOAN_CONTINGENT_USAGE.Add(data);
            if (context.SaveChanges() > 0)
            {
                var application = context.TBL_LMSR_APPLICATION_DETAIL.Where(o => o.LOANREVIEWAPPLICATIONID == entity.loanReviewApplicationId).Select(o => o).FirstOrDefault();
                application.OPERATIONPERFORMED = true;

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.CONTINGENTLOANUSAGEID;
                workflow.Comment = "APS Request approval";
                workflow.OperationId = (int)OperationsEnum.APSReleaseAPGApproval;
                //workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                return true;
            }

            return false;
        }
        public bool SendEmailToBGDesk(int companyId, int staffId, short branchId, string facilityRefNumber)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForBGDesk = (from x in context.TBL_MONITORING_ALERT_SETUP
                                                              where x.MONITORING_ITEMID == (int)AlertMessageEnum.BGDesk
                                                              select x).FirstOrDefault();
            //var messageBody = "A recovery email";
            string messageBody = "Dear Team, <br /><br />This is to bring your attention that an APS Release has Been Initiated on " + $" { DateTime.Today.Date }. with Facility Reference Number" + $"{ facilityRefNumber }  <br /> <br />";

            var subject = "APS RELEASE REQUEST";
            if (string.IsNullOrEmpty(facilityRefNumber)) return false;

            //var email = (from m in context.TBL_ACCREDITEDCONSULTANT
            //             where m.COMPANYID == companyId && m.ACCREDITEDCONSULTANTID == accreditedConsultantId
            //             select new { m.EMAILADDRESS }).FirstOrDefault();
            string templateUrl = @"~/EmailTemplates/Monitoring.html";
            string mailBody = EmailHelpers.PopulateBody(messageBody, templateUrl);

            var emailLog = new TBL_MESSAGE_LOG
            {
                DATETIMERECEIVED = DateTime.Now,
                TOADDRESS = $"{alertsetupForBGDesk.RECIPIENTEMAILS1.Trim()}",
                FROMADDRESS = ConfigurationManager.AppSettings["SupportEmailAddr"],
                MESSAGEBODY = mailBody,
                MESSAGESUBJECT = subject,
                MESSAGESTATUSID = 1,
                MESSAGETYPEID = 1,
                OPERATIONID = (int)OperationsEnum.ContingentLiabilityUsage,
                SENDONDATETIME = DateTime.Now,

            };

            context.TBL_MESSAGE_LOG.Add(emailLog);


            auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ContingentLoanUsageAdd,
                STAFFID = staffId,
                BRANCHID = branchId,
                DETAIL = $"An APS Release Email has been sent to B & G Desk ",
                // IPADDRESS = model.userIPAddress,
                //URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            });

            if (context.SaveChanges() > 0) return true;

            return false;
        }

        public IEnumerable<ContingentLoansViewModel> GetPendingRequest(int staffId)
        {
            return GetRequestWaitingApprovalByOperation(staffId).ToList();
        }

        public List<ContingentLoansViewModel> GetRequestWaitingApprovalByOperation(int staffId)
        {
            //int[] operations = { (int)OperationsEnum.APS_RelaseChecklist, (int)OperationsEnum.APS_ReleaseCAP, (int)OperationsEnum.APS_ReleasePrincipaRequest };

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.APSReleaseAPGApproval).ToList();

            var applications = (from lcu in context.TBL_LOAN_CONTINGENT_USAGE
                               join d in context.TBL_LMSR_APPLICATION_DETAIL on lcu.LOANREVIEWAPPLICATIONID equals d.LOANREVIEWAPPLICATIONID
                               join l in context.TBL_LMSR_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                               join atrail in context.TBL_APPROVAL_TRAIL on lcu.CONTINGENTLOANUSAGEID equals atrail.TARGETID
                               where atrail.OPERATIONID == (int)OperationsEnum.APSReleaseAPGApproval
                               && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                               || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                               || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Authorised)
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && atrail.RESPONSESTAFFID == null
                               && d.OPERATIONPERFORMED == true
                               && (atrail.TOSTAFFID == staffId || atrail.TOSTAFFID == null)

                                orderby lcu.CONTINGENTLOANUSAGEID descending
                               select new ContingentLoansViewModel
                               {
                                   contingentLoanUsageId = lcu.CONTINGENTLOANUSAGEID,
                                   principalName = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                                   bookingDate = lcu.TBL_LOAN_CONTINGENT.BOOKINGDATE,
                                   casaAccountNumber = lcu.TBL_LOAN_CONTINGENT.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   facilityAmount = lcu.TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT,
                                   contingentLoanId = lcu.TBL_LOAN_CONTINGENT.CONTINGENTLOANID,
                                   currencyCode = lcu.TBL_LOAN_CONTINGENT.TBL_CURRENCY.CURRENCYCODE,
                                   currencyId = lcu.TBL_LOAN_CONTINGENT.CURRENCYID,
                                   customerId = lcu.TBL_LOAN_CONTINGENT.CUSTOMERID,
                                   firstName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.FIRSTNAME,
                                   lastName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.LASTNAME,
                                   middleName = lcu.TBL_LOAN_CONTINGENT.TBL_CUSTOMER.MIDDLENAME,
                                   productId = lcu.TBL_LOAN_CONTINGENT.PRODUCTID,
                                   effectiveDate = lcu.TBL_LOAN_CONTINGENT.EFFECTIVEDATE,
                                   exchangeRate = lcu.TBL_LOAN_CONTINGENT.EXCHANGERATE,
                                   loanApplicationReferenceNumber = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   loanReferenceNumber = lcu.TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                                   maturityDate = lcu.TBL_LOAN_CONTINGENT.MATURITYDATE,
                                   productName = lcu.TBL_LOAN_CONTINGENT.TBL_PRODUCT.PRODUCTNAME,
                                   loanStatus = lcu.TBL_LOAN_CONTINGENT.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                   //operationName = context.TBL_OPERATIONS.Where(o=>o.OPERATIONID==atrail.OPERATIONID).Select(o=>o.OPERATIONNAME).FirstOrDefault(),
                                   operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == d.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                   amountRequested = lcu.AMOUNTREQUESTED,
                                   timeIn = atrail.SYSTEMARRIVALDATETIME,
                                   loanApplicationNumber = l.APPLICATIONREFERENCENUMBER,
                               }).ToList();

            foreach (var item in applications)
            {
                var usedData = context.TBL_LOAN_CONTINGENT_USAGE.Where(d => d.CONTINGENTLOANID == item.contingentLoanId);
                if (usedData.Any())
                {
                    item.usedAmount = usedData.Sum(c => c.AMOUNTREQUESTED);
                }
            }

            return applications;
        }

        // TO REMOVE
        private bool ApproveAPSRequest(ApproveAPSRequestViewModel entity)
        {
            var contingentLoanRecord = context.TBL_LOAN_CONTINGENT_USAGE.Where(d => d.CONTINGENTLOANUSAGEID == entity.contingenliabilityUsageId);

            var log = new ApproveAPSRequestViewModel
            {
                staffId = entity.staffId,
                operationId = (int)OperationsEnum.ContingentLiabilityUsage,
                targetId = entity.contingenliabilityUsageId,
                companyId = entity.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                comment = entity.comment,
                deferredExecution = true
            };

            LogForApproval(log);



            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                decimal newLienAmount = 0;

                string lienReferenceNumber = string.Empty;

                if (contingentLoanRecord.Count() == 0)
                {
                    lienReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER;
                }
                else
                {
                    //lienReferenceNumber =  contingentLoanRecord.OrderByDescending(c=> c.CONTINGENTLOANUSAGEID).FirstOrDefault().LIENREFERENCENUMBER;
                }

                decimal oldLien = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT;

                var casaAccountId = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.CASAACCOUNTID;

                var lienModel = new CasaLienViewModel
                {
                    productAccountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                    sourceReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                    userBranchId = (short)entity.BranchId,
                    branchId = (short)entity.BranchId,
                    companyId = entity.companyId,
                    lienAmount = oldLien,
                    description = "Release Lien for APS Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                };

                casaLien.ReleaseLien(lienModel);


                lienReferenceNumber = string.Concat(lienReferenceNumber, contingentLoanRecord.Count());
                newLienAmount = oldLien - contingentLoanRecord.FirstOrDefault().AMOUNTREQUESTED;
                var lienModel2 = new CasaLienViewModel
                {
                    productAccountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                    sourceReferenceNumber = contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                    userBranchId = (short)entity.BranchId,
                    branchId = (short)entity.BranchId,
                    companyId = entity.companyId,
                    lienAmount = newLienAmount,
                    description = "Place Lien on APG Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                };

                casaLien.PlaceLien(lienModel);
            }


            return this.context.SaveChanges() > 0;
        }

        public bool SaveContigentLoansUsageApproval(ApproveAPSRequestViewModel entity)
        {
            workflow.StaffId = entity.staffId;
            workflow.OperationId = (int)OperationsEnum.APSReleaseAPGApproval;
            workflow.TargetId = entity.targetId;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = entity.approvalStatusId;
            workflow.Comment = entity.comment;
            workflow.DeferredExecution = true;
            workflow.LogActivity();


            var usage = context.TBL_LOAN_CONTINGENT_USAGE.FirstOrDefault(d => d.CONTINGENTLOANUSAGEID == entity.targetId);

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Approved)
            {
                var lien = context.TBL_CASA_LIEN.FirstOrDefault(x => x.SOURCEREFERENCENUMBER == entity.loanReferenceNumber && x.LIENTYPEID == (int)LienTypeEnum.APGBooking);
                if (lien != null)
                { //throw new SecureException("No lien has been placed");
                    string lienReferenceNumber = lien.LIENREFERENCENUMBER;

                    decimal oldLien = usage.TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT;
                    var casaAccountId = usage.TBL_LOAN_CONTINGENT.CASAACCOUNTID;

                    casaLien.ReleaseLien(new CasaLienViewModel
                    {
                        sourceReferenceNumber = entity.loanReferenceNumber,
                        productAccountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                        lienReferenceNumber = lienReferenceNumber,
                        userBranchId = (short)entity.BranchId,
                        branchId = (short)entity.BranchId,
                        companyId = entity.companyId,
                        lienAmount = oldLien,
                        description = lien.DESCRIPTION,
                        lienTypeId = (short)LienTypeEnum.APSRequest,
                        createdBy = entity.createdBy,
                        userIPAddress = entity.userIPAddress,
                        applicationUrl = entity.applicationUrl,
                    }, null, false);

                    decimal newLienAmount = oldLien - usage.AMOUNTREQUESTED;

                    casaLien.PlaceLien(new CasaLienViewModel
                    {
                        sourceReferenceNumber = entity.loanReferenceNumber,
                        productAccountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
                        lienReferenceNumber = usage.TBL_LOAN_CONTINGENT.LOANREFERENCENUMBER,
                        userBranchId = (short)entity.BranchId,
                        branchId = (short)entity.BranchId,
                        companyId = entity.companyId,
                        lienAmount = newLienAmount,
                        description = lien.DESCRIPTION,
                        lienTypeId = (short)LienTypeEnum.APSRequest,
                        createdBy = entity.createdBy,
                        userIPAddress = entity.userIPAddress,
                        applicationUrl = entity.applicationUrl,
                    });
                }

                usage.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
            }

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
            {
                usage.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
            }

            return this.context.SaveChanges() > 0;
        }
        public List<ContingentLoansViewModel> GetContingentUsage(int loanId)
        {

            List<ContingentLoansViewModel> contingentData = new List<ContingentLoansViewModel>();
            var data = (from a in context.TBL_LOAN_CONTINGENT
                        join b in context.TBL_PRODUCT_BEHAVIOUR on a.PRODUCTID equals b.PRODUCTID
                        join c in context.TBL_LOAN_CONTINGENT_USAGE on a.CONTINGENTLOANID equals c.CONTINGENTLOANID
                        where b.ALLOWFUNDUSAGE == true && c.CONTINGENTLOANID == loanId
                        && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                        select new ContingentLoansViewModel()
                        {
                            principalName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                            bookingDate = a.BOOKINGDATE,
                            casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            facilityAmount = a.CONTINGENTAMOUNT,
                            contingentLoanId = a.CONTINGENTLOANID,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                            effectiveDate = a.EFFECTIVEDATE,
                            exchangeRate = a.EXCHANGERATE,
                            loanApplicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            maturityDate = a.MATURITYDATE,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            usedAmount = c.AMOUNTREQUESTED,
                            remark = c.REMARK,

                        }).ToList();

            return data;
        }

        public List<CollateralDocumentViewModel> GetContingentUsageDocument(int loanId)
        {
            var model = new List<CollateralDocumentViewModel>();
            var contingent = context.TBL_LOAN_CONTINGENT_USAGE.Where(o => o.CONTINGENTLOANID == loanId).Select(o => o).ToList();
            if (contingent.Count() < 1)
                return model;

            foreach (var x in contingent)
            {
                var data = documentsContext.TBL_LOAN_CONTINGENT_USAGE_DOCS.Where(o => o.CONTINGENTLOANUSAGEID == x.CONTINGENTLOANUSAGEID).Select(o => new CollateralDocumentViewModel
                {
                    ContingentAmount = x.AMOUNTREQUESTED,
                    fileData = o.FILEDATA,
                    fileExtension = o.FILEEXTENSION,
                    fileName = o.FILENAME,
                    dateTimeCreated = x.DATETIMECREATED,
                    documentId = o.DOCUMENTID,
                    documentTitle = o.PHYSICALLOCATION
                }).FirstOrDefault();

                model.Add(data);
            }
            return model;
        }
    }
}
