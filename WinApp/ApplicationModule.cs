using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities;
using System.Data.Entity;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Repositories.Setups.Finance;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Repositories.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Repositories.CreditLimitValidations;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Repositories.Finance;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using FintrakBanking.Interfaces.Setups.Risk;
using FintrakBanking.Repositories.Setups.Risk;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.Repositories.Risk;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Repositories.Customer;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Repositories.Admin;
using FintrakBanking.Interfaces.Helper;
using FintrakBanking.Repositories.Helper;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Repositories.CASA;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Repositories.ErrorLogger;
using FintrakBanking.Interfaces.AppEmail;
using FintrakBanking.Repositories.AppEmail;
using FintrakBanking.Interfaces.Setups;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Repositories.Setups.Approval;
using FintrakBanking.Interfaces.media;
using FintrakBanking.Repositories.media;
using FintrakBanking.Interfaces.Notification;
using FintrakBanking.Repositories.Notification;
using Ninject;

namespace WinApp
{
    class ApplicationModule: NinjectModule
    {
        public override void Load()
        {
            //Bind(typeof(ILoanScheduleRepository<>)).To(typeof(LoanScheduleRepository<>));
            RegisterServices(Kernel);
        }
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<FinTrakBankingContext>().To<FinTrakBankingContext>();
            kernel.Bind<FinTrakBankingDocumentsContext>().To<FinTrakBankingDocumentsContext>();

            kernel.Bind<IGeneralSetupRepository>().To<GeneralSetupRepository>();
            kernel.Bind<IAuthenticationRepository>().To<AuthenticationRepository>();
            kernel.Bind<IAuthorizationRepository>().To<AuthorizationRepository>();
            kernel.Bind<IChartOfAccountRepository>().To<ChartOfAccountRepository>();
            kernel.Bind<IAccountCategoryRepository>().To<AccountCategoryRepository>();
            kernel.Bind<IAccountTypeRepository>().To<AccountTypeRepository>();
            kernel.Bind<ICompanyRepository>().To<CompanyRepository>();
            kernel.Bind<IBranchRepository>().To<BranchRepository>();
            kernel.Bind<IRiskSetupRepository>().To<RiskSetupRepository>();
            kernel.Bind<IRiskImplementation>().To<RiskImplementation>();
            kernel.Bind<IStaffRepository>().To<StaffRepository>();
            kernel.Bind<IDepartmentRepository>().To<DepartmentRepository>();
            kernel.Bind<IMisInfoRepository>().To<MisInfoRepository>();
            kernel.Bind<ICollateralTypeRepository>().To<CollateralTypeRepository>();
            kernel.Bind<IAccountSensitivityRepository>().To<AccountSensitivityRepository>();
            kernel.Bind<IProductRepository>().To<ProductRepository>();
            kernel.Bind<IJobTitleRepository>().To<JobTitleRepository>();
            kernel.Bind<IRankRepository>().To<RankRepository>();
            kernel.Bind<IProductCollateralTypeRepository>().To<ProductCollateralTypeRepository>();
            kernel.Bind<IProductFeeRepository>().To<ProductFeeRepository>();
            kernel.Bind<ICustomerRepository>().To<CustomerRepository>();
            kernel.Bind<ICustomerGroupRepository>().To<CustomerGroupRepository>();
            kernel.Bind<IAuditTrailRepository>().To<AuditTrailRepository>();
            kernel.Bind<ICultureHelper>().To<CultureHelper>();
            kernel.Bind<ICasaRepository>().To<CasaRepository>();
            kernel.Bind<IErrorLogRepository>().To<ErrorLogRepository>();
            kernel.Bind<IEmailRepository>().To<EmailRepository>();
            kernel.Bind<IAdminRepository>().To<AdminRepository>();
            kernel.Bind<ILoanCovenantRepository>().To<LoanCovenantRepository>();
            kernel.Bind<ICustomerCollateralRepository>().To<CustomerCollateralRepository>();
            kernel.Bind<ICountryRepository>().To<CountryRepository>();
            kernel.Bind<ICustomerFSCaptionGroupRepository>().To<CustomerFSCaptionGroupRepository>();
            kernel.Bind<ICustomerFSCaptionRepository>().To<CustomerFSCaptionRepository>();
            kernel.Bind<ICustomerFSCaptionDetailRepository>().To<CustomerFSCaptionDetailRepository>();
            kernel.Bind<ICustomFieldsRepository>().To<CustomFieldsRepository>();
            kernel.Bind<ICustomerFSRatioRepository>().To<CustomerFSRatioRepository>();
            kernel.Bind<ICurrencyRateRepository>().To<CurrencyRateRepository>();
            kernel.Bind<IChecklistRepository>().To<ChecklistRepository>();
            kernel.Bind<ILoanRepository>().To<LoanRepository>();
            kernel.Bind<ICurrencyRateRepository>().To<CurrencyRateRepository>();
            kernel.Bind<IApprovalGroupMappingRepository>().To<ApprovalGroupMappingRepository>();
            //kernel.Bind<IWorkFlowRepository>().To<WorkFlowRepository>();
            kernel.Bind<IWorkflow>().To<Workflow>();
            kernel.Bind<IApprovalGroupRepository>().To<ApprovalGroupRepository>();
            kernel.Bind<IApprovalLevelRepository>().To<ApprovalLevelRepository>();
            kernel.Bind<IApprovalLevelStaffRepository>().To<ApprovalLevelStaffRepository>();
            kernel.Bind<ILoanApplicationRepository>().To<LoanApplicationRepository>();
            kernel.Bind<IMediaRepository>().To<MediaRepository>();
            kernel.Bind<ICanAuthorizationRepository>().To<CanAuthorizationRepository>();
            kernel.Bind<INotificationRepository>().To<NotificationRepository>();
            kernel.Bind<ITaxRepository>().To<TaxRepository>();
            kernel.Bind<IChargeFeeRepository>().To<ChargeFeeRepository>();
            kernel.Bind<ILoanScheduleRepository>().To<LoanScheduleRepository>();
            kernel.Bind<IAppraisalMemorandumRepository>().To<AppraisalMemorandumRepository>();
            kernel.Bind<ICreditTemplateRepository>().To<CreditTemplateRepository>();
            kernel.Bind<ILoanDocumentRepository>().To<LoanDocumentRepository>();
            kernel.Bind<ICollateralDocumentRepository>().To<CollateralDocumentRepository>();
            kernel.Bind<IJobRequestRepository>().To<JobRequestRepository>();
            kernel.Bind<ILoanPreliminaryEvaluationRepository>().To<LoanPreliminaryEvaluationRepository>();
            kernel.Bind<ILimitRepository>().To<LimitRepository>();
            kernel.Bind<IFinanceTransactionRepository>().To<FinanceTransactionRepository>();
            kernel.Bind<ILoanOperationsRepository>().To<LoanOperationsRepository>();
            kernel.Bind<ICreditLimitValidationsRepository>().To<CreditLimitValidationsRepository>();
            kernel.Bind<IPublicHolidayRepository>().To<PublicHolidayRepository>();
            kernel.Bind<IAccreditedConsultantsRepository>().To<AccreditedConsultantsRepository>();
            kernel.Bind<ICallMemoRepository>().To<CallMemoRepository>();
            kernel.Bind<IConditionPrecedentRepository>().To<ConditionPrecedentRepository>();
            kernel.Bind<ICustomerStagingRepository>().To<CustomerStagingRepository>();
            
        }
    }
    
}
