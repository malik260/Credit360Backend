using FintrakBanking.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.ComponentModel.Composition;
using FintrakBanking.ViewModels.Setups;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ILoanCovenantRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanCovenantRepository : ILoanCovenantRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        //private int customerId;
        //int status = 0;

        public LoanCovenantRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository _genSetup,
                                    FinTrakBankingContext _context)
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        #region LoanCovenantDetail
        public async Task<int> AddMultipleLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel)
        {
            if (covenantModel.Count <= 0)
                return -1;

            foreach (LoanCovenantDetailViewModel entity in covenantModel)
            {
                AddLoanCovenantDetail(entity);
            }

            return 1;

        }

        public bool AddLoanCovenantDetail(LoanCovenantDetailViewModel entity)
        {
            var convenant = new TBL_LOAN_COVENANT_DETAIL
            {
                COMPANYID = entity.companyId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDATE = entity.covenantDate,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTTYPEID = entity.covenantTypeId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = this.genSetup.GetApplicationDate(),
                FREQUENCYTYPEID = entity.frequencyTypeId,
                LOANID = entity.loanId
            };
            context.TBL_LOAN_COVENANT_DETAIL.Add(convenant);

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == entity.loanId).LOANREFERENCENUMBER;

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
            //    STAFFID = entity.createdBy,
            //    BRANCHID = (short)entity.userBranchId,
            //    DETAIL = $"Added loan convent to loan ref: { loanRef } ",
            //    IPADDRESS = entity.userIPAddress,
            //    URL = entity.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteLoanCovenantDetail(int loanCovenantDetailId, UserInfo user)
        {
            var convenant = context.TBL_LOAN_COVENANT_DETAIL.Find(loanCovenantDetailId);
            convenant.DELETED = true;
            convenant.DELETEDBY = user.staffId;
            convenant.DATETIMEDELETED = this.genSetup.GetApplicationDate().Date;

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == loanCovenantDetailId).LOANREFERENCENUMBER;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan convent to loan ref: { loanRef } ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<LoanCovenantDetailViewModel> LoanCovenantDetail(int companyId)
        {
            return context.TBL_LOAN_COVENANT_DETAIL.Where(c => c.COMPANYID == companyId).Select(c => new LoanCovenantDetailViewModel
            {
                covenantAmount = c.COVENANTAMOUNT,
                companyId = c.COMPANYID,
                covenantDate = c.COVENANTDATE,
                covenantDetail = c.COVENANTDETAIL,
                covenantTypeId = c.COVENANTTYPEID,
                covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                frequencyTypeId = c.FREQUENCYTYPEID,
                frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                loanId = c.LOANID,
                //loanRef = c.tbl_Loan.LoanReferenceNumber,
                // productName = c.tbl_Loan.tbl_Product.ProductName
            });
        }

        //public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetail(int companyId)
        //{
        //    return LoanCovenantDetail(companyId);
        //}

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByCovenantType(int covenantTypeId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.covenantTypeId == covenantTypeId);
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByloanId(int loanId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.loanId == loanId);
        }

        public async Task<bool> UpdateLoanCovenantDetail(int id, LoanCovenantDetailViewModel entity)
        {
            var convenant = context.TBL_LOAN_COVENANT_DETAIL.Find(id);

            convenant.COMPANYID = entity.companyId;
            convenant.COVENANTAMOUNT = entity.covenantAmount;
            convenant.COVENANTDATE = entity.covenantDate;
            convenant.COVENANTDETAIL = entity.covenantDetail;
            convenant.COVENANTTYPEID = entity.covenantTypeId;
            convenant.CREATEDBY = entity.createdBy;
            convenant.DATETIMEUPDATED = this.genSetup.GetApplicationDate().Date;
            convenant.FREQUENCYTYPEID = entity.frequencyTypeId;
            convenant.LOANID = entity.loanId;

            var loanRef = context.TBL_LOAN.SingleOrDefault(c => c.TERMLOANID == entity.loanId).LOANREFERENCENUMBER;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailUpdate,
                STAFFID = entity.lastUpdatedBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated loan convent to loan ref: { loanRef } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantDetailById(int covenantDetailId, int companyId)
        {
            return LoanCovenantType(companyId).Where(c => c.covenantTypeId == covenantDetailId);
        }

        #endregion LoanCovenantDetail

        #region Loan Covenant Type
        public async Task<bool> AddLoanCovenantType(LoanCovenantTypeViewModel entity)
        {
            var convenant = new TBL_LOAN_COVENANT_TYPE
            {
                COMPANYID = entity.companyId,
                COVENANTTYPENAME = entity.covenantTypeName,
                REQUIREAMOUNT = entity.requireAmount,
                REQUIREFREQUENCY = entity.requireFrequency
            };
            context.TBL_LOAN_COVENANT_TYPE.Add(convenant);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantTypeAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Defined loan convent type: { entity.covenantTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateLoanCovenantType(short id, LoanCovenantTypeViewModel entity)
        {
            var convenant = context.TBL_LOAN_COVENANT_TYPE.Find(id);
            convenant.COMPANYID = entity.companyId;
            convenant.COVENANTTYPENAME = entity.covenantTypeName;
            convenant.REQUIREAMOUNT = entity.requireAmount;
            convenant.REQUIREFREQUENCY = entity.requireFrequency;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantTypeAdd,
                STAFFID = entity.lastUpdatedBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated loan convent type: { entity.covenantTypeName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            //this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        IEnumerable<LoanCovenantTypeViewModel> LoanCovenantType(int companyId)
        {
            return context.TBL_LOAN_COVENANT_TYPE.Where(c => c.COMPANYID == companyId).Select(c => new LoanCovenantTypeViewModel
            {
                companyId = c.COMPANYID,
                covenantTypeId = c.COVENANTTYPEID,
                covenantTypeName = c.COVENANTTYPENAME,
                requireAmount = c.REQUIREAMOUNT,
                requireFrequency = c.REQUIREFREQUENCY,
                requireCasaAccount = c.REQUIRECASAACCOUNT
            });
        }

        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantType(int companyId)
        {
            return LoanCovenantType(companyId);
        }

        // application 

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenant(int applicationId)
        {
            var ids = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => x.LOANAPPLICATIONDETAILID);

            return context.TBL_LOAN_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && ids.Contains(x.LOANAPPLICATIONDETAILID)
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME

                });
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenant(int applicationDetailId)
        {
            return context.TBL_LOAN_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && x.LOANAPPLICATIONDETAILID == applicationDetailId
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME

                });
        }



        public bool UpdateLoanApplicationCovenant(DateTime date)
        {
            var covenants = context.TBL_LOAN_APPLICATION_COVENANT.Where(o => o.NEXTCOVENANTDATE == date).ToList();

            foreach (var covenant in covenants)
            {
                covenant.PREVIOUSCOVENANTDATE = (DateTime)covenant.NEXTCOVENANTDATE;
                covenant.NEXTCOVENANTDATE = GetFrequencyDate((int)covenant.FREQUENCYTYPEID, (DateTime)covenant.NEXTCOVENANTDATE);
            }

            return context.SaveChanges() != 0;
        }


        public bool AddLoanApplicationCovenant(LoanCovenantDetailViewModel entity)
        {
            var convenant = new TBL_LOAN_APPLICATION_COVENANT
            {
                LOANCOVENANTDETAILID = entity.loanCovenantDetailId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDATE = entity.covenantDate,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTTYPEID = entity.covenantTypeId,
                FREQUENCYTYPEID = entity.frequencyTypeId,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                ISPERCENTAGE = entity.isPercentage,
                NEXTCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate), //entity.nextCovenantDate,
                PREVIOUSCOVENANTDATE = GetFrequencyDate((int)entity.frequencyTypeId, entity.covenantDate), //entity.nextCovenantDate,
                CASAACCOUNTID = entity.casaAccountId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = this.genSetup.GetApplicationDate().Date,
                COMPANYID = entity.companyId,
            };
            context.TBL_LOAN_APPLICATION_COVENANT.Add(convenant);

            var appl = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added loan application covenant on application: { appl.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool DeleteLoanApplicationCovenant(int covenantId, UserInfo user)
        {
            var covenant = context.TBL_LOAN_APPLICATION_COVENANT.Find(covenantId);
            covenant.DELETED = true;
            covenant.DELETEDBY = user.staffId;
            covenant.DATETIMEDELETED = DateTime.Now;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan application covenant: { covenant.COVENANTDETAIL } ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        // end application

        #endregion Loan Covenant Type


        #region Begin FrequencyType

        public DateTime GetFrequencyDate(int frequencyTypeId, DateTime date)
        {
            DateTime nextDate = new DateTime();

            if (frequencyTypeId == (int)FrequencyTypeEnum.Yearly)
            {
                nextDate = date.AddMonths(12);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.TwiceYearly)
            {
                nextDate = date.AddMonths(6);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Quarterly)
            {
                nextDate = date.AddMonths(3);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.SixTimesYearly)
            {
                nextDate = date.AddMonths(2);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Monthly)
            {
                nextDate = date.AddMonths(1);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.ThriceYearly)
            {
                nextDate = date.AddMonths(4);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Daily)
            {
                nextDate = date.AddDays(1);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.Weekly)
            {
                nextDate = date.AddDays(7);
            }
            else if (frequencyTypeId == (int)FrequencyTypeEnum.TwiceMonthly)
            {
                nextDate = date.AddDays(14);
            }
            return nextDate;
        }

        #endregion End FrequencyType

        #region LMS APPROVAL


        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationCovenantLms(int applicationId)
        {
            var ids = context.TBL_LMSR_APPLICATION_DETAIL
                 .Where(x => x.LOANAPPLICATIONID == applicationId)
                 .Select(x => x.LOANREVIEWAPPLICATIONID);

            return context.TBL_LMSR_APPLICATION_COVENANT.Where(x =>
                    x.DELETED == false && ids.Contains(x.LOANREVIEWAPPLICATIONID)
                ).Select(c => new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = c.LOANCOVENANTDETAILID,
                    covenantAmount = c.COVENANTAMOUNT,
                    covenantDate = c.COVENANTDATE,
                    covenantDetail = c.COVENANTDETAIL,
                    covenantTypeId = c.COVENANTTYPEID,
                    covenantTypeName = c.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                    frequencyTypeId = c.FREQUENCYTYPEID,
                    frequencyTypeName = c.TBL_FREQUENCY_TYPE.MODE,
                    loanApplicationDetailId = c.LOANREVIEWAPPLICATIONID,
                    isPercentage = c.ISPERCENTAGE,
                    nextCovenantDate = c.NEXTCOVENANTDATE,
                    casaAccountId = c.CASAACCOUNTID,

                    companyId = c.COMPANYID,
                    productCustomerName = c.TBL_LMSR_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_LMSR_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME

                });
        }

        public bool AddLoanApplicationCovenantLms(LoanCovenantDetailViewModel entity)
        {
            var convenant = new TBL_LMSR_APPLICATION_COVENANT
            {
                LOANCOVENANTDETAILID = entity.loanCovenantDetailId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDATE = entity.covenantDate,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTTYPEID = entity.covenantTypeId,
                FREQUENCYTYPEID = entity.frequencyTypeId,
                LOANREVIEWAPPLICATIONID = entity.loanApplicationDetailId,
                ISPERCENTAGE = entity.isPercentage,
                NEXTCOVENANTDATE = entity.nextCovenantDate,
                CASAACCOUNTID = entity.casaAccountId,

                CREATEDBY = entity.createdBy,
                DATETIMECREATED = this.genSetup.GetApplicationDate().Date,
                COMPANYID = entity.companyId,
            };
            context.TBL_LMSR_APPLICATION_COVENANT.Add(convenant);

            var appl = context.TBL_LMSR_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailAdd,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added loan REVIEW application covenant on application: { appl.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool DeleteLoanApplicationCovenantLms(int covenantId, UserInfo user)
        {
            var covenant = context.TBL_LMSR_APPLICATION_COVENANT.Find(covenantId);
            covenant.DELETED = true;
            covenant.DELETEDBY = user.staffId;
            covenant.DATETIMEDELETED = DateTime.Now;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete loan REVIEW application covenant: { covenant.COVENANTDETAIL } ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        #endregion LMS APPROVAL

    }
}
