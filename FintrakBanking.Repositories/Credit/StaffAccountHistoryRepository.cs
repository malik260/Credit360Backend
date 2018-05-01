using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Entities.Models;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.WorkFlow;
using System.Data.Entity.Validation;
using FintrakBanking.ViewModels.WorkFlow;
using System.Data.Entity;

namespace FintrakBanking.Repositories.Credit
{
    public class StaffAccountHistoryRepository : IStaffAccountHistoryRepository
    {
        private IWorkflow workflow;
        private IGeneralSetupRepository genSetup;
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;
        private ILoanOperationsRepository loanOp;

        public StaffAccountHistoryRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository genSetup,
            IAuditTrailRepository auditTrail,
            IGeneralSetupRepository generalSetup,
            ILoanOperationsRepository loanOp,
            IWorkflow workflow)
        {
            this.context = context;
            this.genSetup = genSetup;
            this.auditTrail = auditTrail;
            this.workflow = workflow;
            this.generalSetup = generalSetup;
            this.loanOp = loanOp;
        }


        public IEnumerable<StaffAccountHistoryViewModel> GetAllStaffAccountHistory()
        {
            List<StaffAccountHistoryViewModel> accountLst = new List<StaffAccountHistoryViewModel>();
            var data = context.TBL_STAFF_ACCOUNT_HISTORY.Select(sa => new StaffAccountHistoryViewModel
            {
                targetId = sa.TARGETID,
                currentRMStaffId = sa.STAFFID,
                approvalStatusId = sa.APPROVALSTATUSID,
                reasonForChange = sa.REASONFORCHANGE,
                staffAccountHistoryId = sa.STAFFACCOUNTHISTORYID,
                productType = sa.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                endDate = sa.ENDDATE,
                startDate = sa.STARTDATE,
                productTypeId = sa.PRODUCTTYPEID,
                staffId = sa.STAFFID,
                newRMStaffId = sa.NEWSTAFFID,
            }).ToList();

            foreach (var d in data)
            {
                d.newRMStaffName = d.staffId>0 ? GetStaff(d.newRMStaffId) : null;
                d.currentRMStaffName = GetStaff(d.currentRMStaffId);

                accountLst.Add(d);
            }
            return accountLst;

        }

        public bool AddStaffAccountHistory(StaffAccountHistoryViewModel entity)
        {
            var checkStartDate = context.TBL_STAFF_ACCOUNT_HISTORY.Where(c => c.NEWSTAFFID == entity.currentRMStaffId
            && c.PRODUCTTYPEID == entity.productTypeId
            && c.TARGETID == entity.targetId);
            if (checkStartDate.Any())
            {
                entity.startDate = checkStartDate.FirstOrDefault().ENDDATE;
            }
            else
            {
                var getLoan = GetloanDetails(entity.targetId, entity.staffId, entity.productTypeId);
                entity.startDate = getLoan.effectiveDate;
            }

            var data = new TBL_STAFF_ACCOUNT_HISTORY
            {
                TARGETID = entity.targetId,
                STAFFID = entity.currentRMStaffId,
                NEWSTAFFID = entity.newRMStaffId,
                REASONFORCHANGE = entity.reasonForChange,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                ENDDATE = entity.endDate,
                STARTDATE = entity.startDate,
                PRODUCTTYPEID = entity.productTypeId

            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.InitiatAccountReassigning,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Initial account reassigned",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.STAFFACCOUNTHISTORYID
            };


            bool output = false;
            context.TBL_STAFF_ACCOUNT_HISTORY.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            try
            {
                output = context.SaveChanges() > 0;
                //  response = context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {

                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }

            // ----------------Drop into CAM-------------------
            workflow.StaffId = entity.staffId;
            workflow.OperationId = (int)OperationsEnum.ReassigningOfAccount;
            workflow.TargetId = data.STAFFACCOUNTHISTORYID;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "Initiation: " + entity.reasonForChange;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
            // ----------------Drop into CAM ends-------------------

            return context.SaveChanges() > 0;


        }
       
        public IEnumerable<StaffAccountHistoryViewModel> GetStaffAccountHistory(int staffId)
        {
            try
            {
                List<StaffAccountHistoryViewModel> accountLst = new List<StaffAccountHistoryViewModel>();
                var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ReassigningOfAccount).ToList();


                var data = (from ln in context.TBL_STAFF_ACCOUNT_HISTORY

                            join atrail in context.TBL_APPROVAL_TRAIL on ln.STAFFACCOUNTHISTORYID equals atrail.TARGETID
                            where
                            (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                   && atrail.OPERATIONID == (int)OperationsEnum.ReassigningOfAccount
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.STAFFACCOUNTHISTORYID descending
                            select new StaffAccountHistoryViewModel
                            {
                                staffAccountHistoryId = ln.STAFFACCOUNTHISTORYID,
                                reasonForChange = ln.REASONFORCHANGE,
                                startDate = ln.STARTDATE,
                                endDate = ln.ENDDATE,
                                newRMStaffId = ln.NEWSTAFFID,
                                currentRMStaffId = ln.STAFFID,
                                targetId = ln.TARGETID,
                                productTypeId = ln.PRODUCTTYPEID
                            }).ToList();


                foreach (var d in data)
                {
                    d.newRMStaffName = GetStaff(d.newRMStaffId);
                    d.currentRMStaffName = GetStaff(d.currentRMStaffId);

                    accountLst.Add(d);
                }
                return accountLst;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public StaffMISHistoryViewModel GetSelectedApprovalLoanDetails(ReasignedAccountApprovalViewModel entity)
        {
            StaffMISHistoryViewModel data = null;
            if (entity != null)
            {
                var staff = context.TBL_STAFF_ACCOUNT_HISTORY.Where(s => s.STAFFACCOUNTHISTORYID == entity.staffAccountHistoryId).FirstOrDefault();

                data = GetSelectedLoanDetails(entity.companyId, staff.TARGETID, entity.productTypeId);
                 
                data.reasonForChange = staff.REASONFORCHANGE;
                data.newRMStaffName = staff.TBL_STAFF1.LASTNAME + " " + staff.TBL_STAFF1.FIRSTNAME + " " + staff.TBL_STAFF1.MIDDLENAME;
                data.endDate = staff.ENDDATE;

            }

            return data;
        }

        public StaffMISHistoryViewModel GetSelectedLoanDetails(int companyId, int loanId, int productTypeId)
        {
            StaffMISHistoryViewModel loan = null;
            if (productTypeId == 1)
            {
                loan = GetRunningTeamLoans(companyId, loanId);
            }
            if(productTypeId == 2 || productTypeId == 6)
            {
                loan = GetRunningRevolvingLoans(companyId, loanId);
            }
            if (productTypeId == 3 || productTypeId == 9)
            {
                loan = GetRunningRevolvingLoans(companyId, loanId);
            }

            return loan;
        }

        public bool ApproveStaffAccountHistory(ReasignedAccountApprovalViewModel entity)
        {
            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
            workflow.TargetId = entity.targetId;
            workflow.Comment = entity.comment;
            workflow.OperationId =(int)OperationsEnum.ReassigningOfAccount;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = false;
            workflow.LogActivity();
 

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if(entity.approvalStatusId == (int)ApprovalStatusEnum.Approved)
                {
                    switch (entity.productTypeId)
                    {
                        case ((int)LoanProductTypeEnum.TermLoan): TeamLoan(entity); break;
                        case ((int)LoanProductTypeEnum.RevolvingLoan): RevolvingLoan(entity); break;
                    }

                    var data = context.TBL_STAFF_ACCOUNT_HISTORY.FirstOrDefault(sa => sa.APPROVALSTATUSID == entity.approvalStatusId);
                    data.APPROVALSTATUSID = entity.approvalStatusId;
                }
                if (entity.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    var data = context.TBL_STAFF_ACCOUNT_HISTORY.FirstOrDefault(sa => sa.APPROVALSTATUSID == entity.approvalStatusId);
                    data.APPROVALSTATUSID = entity.approvalStatusId;
                }
            }
            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                var data = context.TBL_STAFF_ACCOUNT_HISTORY.FirstOrDefault(sa => sa.APPROVALSTATUSID == entity.approvalStatusId);
                data.APPROVALSTATUSID = entity.approvalStatusId;
            }






                return context.SaveChanges() > 0;
        }


        private string GetStaff(int staffId)
        {
            return context.TBL_STAFF.Where(s => s.STAFFID == staffId).Select(s => new { staffName = s.LASTNAME + " " + s.FIRSTNAME + " " + s.MIDDLENAME }).FirstOrDefault().staffName;
        }

        private loanDetailsViewModel GetloanDetails(int loanId, int staffId, int productTypeId)
        {
            loanDetailsViewModel loanDetails = null;
            switch (productTypeId)
            {
                case ((int)LoanProductTypeEnum.TermLoan):
                    loanDetails = context.TBL_LOAN.Where(l => l.RELATIONSHIPOFFICERID == staffId && l.TERMLOANID == loanId).Select(l => new loanDetailsViewModel
                    {
                        loanId = l.TERMLOANID,
                        relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                        effectiveDate = l.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
                case ((int)LoanProductTypeEnum.RevolvingLoan):
                    loanDetails = context.TBL_LOAN_REVOLVING.Where(r => r.RELATIONSHIPMANAGERID == staffId && r.REVOLVINGLOANID == loanId).Select(r => new loanDetailsViewModel
                    {
                        loanId = r.REVOLVINGLOANID,
                        relationshipOfficerId = r.RELATIONSHIPOFFICERID,
                        effectiveDate = r.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
                case ((int)LoanProductTypeEnum.ContingentLiability):
                    loanDetails = context.TBL_LOAN_CONTINGENT.Where(c => c.RELATIONSHIPMANAGERID == staffId && c.CONTINGENTLOANID == loanId).Select(c => new loanDetailsViewModel
                    {
                        loanId = c.CONTINGENTLOANID,
                        relationshipOfficerId = c.RELATIONSHIPOFFICERID,
                        effectiveDate = c.EFFECTIVEDATE,
                        productTypeId = productTypeId
                    }).FirstOrDefault(); break;
            }
            return loanDetails;
        }


        private StaffMISHistoryViewModel GetRunningTeamLoans(int companyId, int loanId)
        {
            var applicationDate = generalSetup.GetApplicationDate();


            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == loanId && x.COMPANYID == companyId);
            DateTime maturityDate = loan.MATURITYDATE;
            DateTime effectiveDate = loan.EFFECTIVEDATE;
            TimeSpan difference = maturityDate - applicationDate;
            int days = (int)difference.TotalDays;

            decimal accruedInterest = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.DATE == DbFunctions.TruncateTime(applicationDate.Date)).ACCRUEDINTEREST;
            accruedInterest = decimal.Round(accruedInterest, 2, MidpointRounding.AwayFromZero);
            decimal outStandingBalance = loan.OUTSTANDINGPRINCIPAL;
            decimal pastDue = decimal.Round((loan.PASTDUEINTEREST + loan.PASTDUEPRINCIPAL + loan.INTERESTONPASTDUEINTEREST + loan.INTERESTONPASTDUEPRINCIPAL), 2, MidpointRounding.AwayFromZero);
            DateTime nextPaymentDate =context. TBL_LOAN_SCHEDULE_PERIODIC.FirstOrDefault(x => x.LOANID == loan.TERMLOANID && x.PAYMENTDATE >= DbFunctions.TruncateTime( applicationDate.Date)).PAYMENTDATE;


            decimal totalamount = (accruedInterest + outStandingBalance + pastDue);


            var runningLoan = new StaffMISHistoryViewModel
            {
                loanId = loan.TERMLOANID,
                companyName = loan.TBL_COMPANY.NAME,
                companyId = loan.COMPANYID,
                customerName = loan.TBL_CUSTOMER.FIRSTNAME + " " + loan.TBL_CUSTOMER.MIDDLENAME + " " + loan.TBL_CUSTOMER.LASTNAME,
                customerId = loan.CUSTOMERID,
                approvedAmount = loan.PRINCIPALAMOUNT,
                branchName = loan.TBL_BRANCH.BRANCHNAME,
                interestRate = loan.INTERESTRATE,
                outstandingInterest = loan.OUTSTANDINGINTEREST,
                outstandingPrincipal = loan.OUTSTANDINGPRINCIPAL,
                principalAmount = loan.PRINCIPALAMOUNT,
                currency = loan.TBL_CURRENCY.CURRENCYNAME,
                loanReferenceNumber = loan.LOANREFERENCENUMBER,
                effectiveDate = applicationDate,//DateTime.Now,
                previousEffectiveDate = loan.EFFECTIVEDATE,
                equityContribution = 0,
                maintainTenor = true,
                maturityDate = loan.MATURITYDATE,
                scheduleTypeId = loan.SCHEDULETYPEID,
                scheduleTypeCategoryId = loan.TBL_LOAN_SCHEDULE_TYPE.SCHEDULECATEGORYID,
                teno = days,
                newtenor = 0,
                accrualedAmount = accruedInterest,
                totalAmount = totalamount,
                firstPrincipalPaymentDate = nextPaymentDate,
                firstInterestPaymentDate = nextPaymentDate,
                principalFrequencyTypeId = loan.PRINCIPALFREQUENCYTYPEID,
                interestFrequencyTypeId = loan.INTERESTFREQUENCYTYPEID,
                pastDueTotal = pastDue,
                relationshipManagerId = loan.RELATIONSHIPMANAGERID,
                relationshipOfficerId = loan.RELATIONSHIPOFFICERID,
                productTypeId = loan.TBL_PRODUCT.PRODUCTTYPEID
            };


            return runningLoan;
        }

        private StaffMISHistoryViewModel GetRunningRevolvingLoans(int companyId, int loanId)
        {
            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               where a.REVOLVINGLOANID == loanId && a.ISDISBURSED == true && a.COMPANYID == companyId
                               select new StaffMISHistoryViewModel
                               {
                                    
                                   loanId = a.REVOLVINGLOANID,
                                   loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                   customerId = a.CUSTOMERID,
                                   customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                   customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = a.TBL_BRANCH.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                   productName = a.TBL_PRODUCT.PRODUCTNAME,
                                   productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,

                                   isDisbursed = a.ISDISBURSED,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                   loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.REVOLVINGLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME
                               }).FirstOrDefault();
            return loanDetails;
        }

        private StaffMISHistoryViewModel GetRunningContingentLiability(int companyId, int loanId)
        {
            var loan = context.TBL_LOAN_CONTINGENT.FirstOrDefault(c => c.CONTINGENTLOANID == loanId && c.COMPANYID == companyId);

           var amountDisburst = loan.TBL_LOAN_CONTINGENT_USAGE.Where(c=> c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Sum(c => c.AMOUNTREQUESTED);

            var runningLoan = new StaffMISHistoryViewModel
            {
                loanId = loan.CONTINGENTLOANID,
                companyName = loan.TBL_COMPANY.NAME,
                companyId = loan.COMPANYID,
                customerName = loan.TBL_CUSTOMER.FIRSTNAME + " " + loan.TBL_CUSTOMER.MIDDLENAME + " " + loan.TBL_CUSTOMER.LASTNAME,
                customerId = loan.CUSTOMERID,
                approvedAmount = loan.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                branchName = loan.TBL_BRANCH.BRANCHNAME,

                currency = loan.TBL_CURRENCY.CURRENCYNAME,
                loanReferenceNumber = loan.LOANREFERENCENUMBER,
                effectiveDate = loan.EFFECTIVEDATE,//DateTime.Now,
                previousEffectiveDate = loan.EFFECTIVEDATE,
                equityContribution = 0,
                maintainTenor = true,
                maturityDate = loan.MATURITYDATE,
                disbursableAmount = amountDisburst,
                newtenor = 0,
                accrualedAmount = 0,

                relationshipManagerId = loan.RELATIONSHIPMANAGERID,
                relationshipOfficerId = loan.RELATIONSHIPOFFICERID,
                productTypeId = loan.TBL_PRODUCT.PRODUCTTYPEID
            };
            return runningLoan;
        }



        private void TeamLoan(ReasignedAccountApprovalViewModel entity )
        {
            var data = context.TBL_LOAN.FirstOrDefault(l => l.TERMLOANID == entity.loanId);
            loanOp.ArchiveLoan(entity.targetId, (int)OperationsEnum.ReassigningOfAccount);

            data.RELATIONSHIPOFFICERID = entity.newRMStaffId;
            
        }
        private void RevolvingLoan(ReasignedAccountApprovalViewModel entity)
        {
            var data = context.TBL_LOAN_REVOLVING.FirstOrDefault(l => l.REVOLVINGLOANID == entity.loanId);
            loanOp.ArchiveLoan(entity.targetId, (int)OperationsEnum.ReassigningOfAccount);
            data.RELATIONSHIPOFFICERID = entity.newRMStaffId;
           
        }
        private void ContingentLiability(ReasignedAccountApprovalViewModel entity)
        {
            var data = context.TBL_LOAN_CONTINGENT.FirstOrDefault(l => l.CONTINGENTLOANID == entity.loanId);
            data.RELATIONSHIPOFFICERID = entity.newRMStaffId;
             
        }

      
    }
    internal class loanDetailsViewModel
    {
        public int loanId { get; set; }
        public int relationshipOfficerId { get; set; }
        public DateTime effectiveDate { get; set; }
        public int productTypeId { get; set; }
    }

   


}
