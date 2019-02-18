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

namespace FintrakBanking.Repositories.Credit
{
    public class ContingentLoanUsageRepository : IContingentLoanUsageRepository
    {
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IWorkflow workflow;
        private IAuditTrailRepository auditTrail;
        private ICasaLienRepository casaLien;

        public object entiry { get; private set; }

        public ContingentLoanUsageRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup, IAuditTrailRepository auditTrail, IWorkflow workflow, ICasaLienRepository casaLien)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.casaLien = casaLien;
        }

        public IEnumerable<ContingentLoansViewModel> GetAllContingentLoans(int staffId, int companyId)
        {
            try
            {
                List<ContingentLoansViewModel> contingentData = new List<ContingentLoansViewModel>();
                DateTime currentDate = genSetup.GetApplicationDate();
                var data = (from a in context.TBL_LOAN_CONTINGENT
                            join b in context.TBL_PRODUCT_BEHAVIOUR on a.PRODUCTID equals b.PRODUCTID
                            where  currentDate <= a.MATURITYDATE && b.ALLOWFUNDUSAGE == true
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
                                loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS
                            }).ToList();

                var data2 = context.TBL_LOAN_CONTINGENT
                    .Where(c => c.MATURITYDATE <= currentDate
                    && context.TBL_PRODUCT_BEHAVIOUR.Where(d => d.PRODUCTID == c.PRODUCTID)
                    .FirstOrDefault().ALLOWFUNDUSAGE == true)
                    .Select(c => new ContingentLoansViewModel()
                    {
                        principalName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME,
                        bookingDate = c.BOOKINGDATE,
                        casaAccountNumber = c.TBL_CASA.PRODUCTACCOUNTNUMBER,
                        facilityAmount = c.CONTINGENTAMOUNT,
                        contingentLoanId = c.CONTINGENTLOANID,
                        currencyCode = c.TBL_CURRENCY.CURRENCYCODE,
                        currencyId = c.CURRENCYID,
                        customerId = c.CUSTOMERID,
                        firstName = c.TBL_CUSTOMER.FIRSTNAME,
                        lastName = c.TBL_CUSTOMER.LASTNAME,
                        middleName = c.TBL_CUSTOMER.MIDDLENAME,
                        productId = c.PRODUCTID,
                        effectiveDate = c.EFFECTIVEDATE,
                        exchangeRate = c.EXCHANGERATE,
                        loanApplicationReferenceNumber = c.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                        loanReferenceNumber = c.LOANREFERENCENUMBER,
                        maturityDate = c.MATURITYDATE,
                        productName = c.TBL_PRODUCT.PRODUCTNAME,
                        loanStatus = c.TBL_LOAN_STATUS.ACCOUNTSTATUS
                    });

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


                return contingentData.Where(c => c.percentageUsed < 100).OrderByDescending(d => d.contingentLoanId).ToList();
            }
            catch (Exception ex)
            {
                throw;
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
            workflow.StatusId = entity.approvalStatusId;
            workflow.Comment = entity.comment;
            workflow.ExternalInitialization = entity.externalInitialization;
            workflow.DeferredExecution = entity.deferredExecution;
            return response = workflow.LogActivity();
        }

        public bool SaveContigentLoans(ContingentLoanUsageViewModel entity, int companyId)
        {
            var data = new TBL_LOAN_CONTINGENT_USAGE
            {
                AMOUNTREQUESTED = entity.amountRequuested,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                CONTINGENTLOANID = entity.contingentLoanId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                REMARK = entity.remark
            };
            context.TBL_LOAN_CONTINGENT_USAGE.Add(data);
            
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ContingentLoanUsageAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Applied for APS for {entity.productName} with reference number: {entity.loanReferenceNumber}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.CONTINGENTLOANUSAGEID
            };

            this.auditTrail.AddAuditTrail(audit);
            bool response = false;
            //--------------------------------------------------
            response = context.SaveChanges() > 0;
            if (response)
            {
                // ----------------Drop into CAM-------------------
                if (data.CONTINGENTLOANUSAGEID > 0)
                {
                    var log = new ApproveAPSRequestViewModel
                    {
                        staffId = entity.staffId,
                        operationId = (int)OperationsEnum.ContingentLiabilityUsage,
                        targetId = data.CONTINGENTLOANUSAGEID,
                        companyId = entity.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        comment = "New APS Request",
                        deferredExecution = false,
                    };

                    response = LogForApproval(log);

                }
            }
            return response;
    } 
            
        public IEnumerable<ContingentLoansViewModel> GetPendingRequest(int staffId)
        {
            return GetRequestWaitingApprovalByOperation(staffId).ToList();
        }

        public IQueryable<ContingentLoansViewModel> GetRequestWaitingApprovalByOperation( int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ContingentLiabilityUsage).ToList();

            var applications = from lcu in context.TBL_LOAN_CONTINGENT_USAGE
                               join atrail in context.TBL_APPROVAL_TRAIL on lcu.CONTINGENTLOANUSAGEID equals atrail.TARGETID
                               where atrail.OPERATIONID == (int)OperationsEnum.ContingentLiabilityUsage
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
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

                                   amountRequested = lcu.AMOUNTREQUESTED,
                               };
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
                comment =entity.comment,               
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

                var casaAccountId =   contingentLoanRecord.FirstOrDefault().TBL_LOAN_CONTINGENT.CASAACCOUNTID;
       
                var lienModel = new CasaLienViewModel
                {
                    productAccountNumber = context.TBL_CASA.FirstOrDefault(c=> c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER,
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


                lienReferenceNumber = string.Concat( lienReferenceNumber, contingentLoanRecord.Count());
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
            workflow.OperationId = (int)OperationsEnum.ContingentLiabilityUsage;
            workflow.TargetId = entity.targetId;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = entity.approvalStatusId;
            workflow.Comment = entity.comment;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            var usage = context.TBL_LOAN_CONTINGENT_USAGE.FirstOrDefault(d => d.CONTINGENTLOANUSAGEID == entity.targetId);

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Approved)
            {
                var lien = context.TBL_CASA_LIEN.FirstOrDefault(x => x.SOURCEREFERENCENUMBER == entity.loanReferenceNumber && (x.LIENTYPEID == (int)LienTypeEnum.APGBooking || x.LIENTYPEID == (int)LienTypeEnum.APGBooking));
                if (lien == null) throw new SecureException("No lien has been placed");
                string lienReferenceNumber = lien.LIENREFERENCENUMBER;

                decimal oldLien = usage.TBL_LOAN_CONTINGENT.CONTINGENTAMOUNT; // ???? 
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
                    description = "Release Lien for APS Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                },null,false);

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
                    description = "Place Lien on APG Fund",
                    lienTypeId = (short)LienTypeEnum.APSRequest,
                    createdBy = entity.createdBy,
                    userIPAddress = entity.userIPAddress,
                    applicationUrl = entity.applicationUrl,
                });

                usage.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
            }

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
            {
                usage.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
            }

            return this.context.SaveChanges() > 0;
        }

    }
}
