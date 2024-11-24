using FintrakBanking.Interfaces;
using FintrakBanking.Repositories;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(FintrakBanking.APICore.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(FintrakBanking.APICore.App_Start.NinjectWebCommon), "Stop")]

namespace FintrakBanking.APICore.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using System.Web.Http;
    using WebApiContrib.IoC.Ninject;
    using FintrakBanking.Entities.Models;
    using FintrakBanking.Entities.DocumentModels;
    using FintrakBanking.Interfaces.Admin;
    using FintrakBanking.Interfaces.AppEmail;
    using FintrakBanking.Interfaces.CASA;
    using FintrakBanking.Interfaces.Credit;
    using FintrakBanking.Interfaces.Customer;
    using FintrakBanking.Interfaces.ErrorLogger;
    using FintrakBanking.Interfaces.Helper;
    using FintrakBanking.Interfaces.Setups;
    using FintrakBanking.Interfaces.Setups.Approval;
    using FintrakBanking.Interfaces.Setups.Credit;
    using FintrakBanking.Interfaces.Setups.Finance;
    using FintrakBanking.Interfaces.Setups.General;
    using FintrakBanking.Interfaces.Setups.Risk;
    using FintrakBanking.Interfaces.WorkFlow;
    using FintrakBanking.Repositories.Admin;
    using FintrakBanking.Repositories.AppEmail;
    using FintrakBanking.Repositories.CASA;
    using FintrakBanking.Repositories.Credit;
    using FintrakBanking.Repositories.Customer;
    using FintrakBanking.Repositories.ErrorLogger;
    using FintrakBanking.Repositories.Helper;
    using FintrakBanking.Repositories.Setups.Approval;
    using FintrakBanking.Repositories.Setups.Finance;
    using FintrakBanking.Repositories.Setups.General;
    using FintrakBanking.Repositories.Setups.Risk;
    using FintrakBanking.Repositories.WorkFlow;
    using FintrakBanking.Repositories.CreditLimitValidations;
    using FintrakBanking.Interfaces.CreditLimitValidations;
    using FintrakBanking.Interfaces.Notification;
    using FintrakBanking.Repositories.Notification;
    using FintrakBanking.Repositories.Finance;
    using FintrakBanking.Interfaces.media;
    using FintrakBanking.Repositories.media;
    using FintrakBanking.Interfaces.Finance;
    using FintrakBanking.Interfaces.Risk;
    using FintrakBanking.Repositories.Risk; 
    using FintrakBanking.ReportObjects.ReportCalls;
    using FintrakBanking.Interfaces.Reports;
    using FintrakBanking.Repositories.Reports;
    using FintrakBanking.Repositories.Setups.Credit;
    using Ninject.Web.Common.WebHost;
    using FinTrakBanking.ThirdPartyIntegration;
    using FintrakBanking.Repositories.Validetion;
    using FintrakBanking.Interfaces.Validation;
    using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
    using FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration;
    using FinTrakBanking.ThirdPartyIntegration.StagingDatabase.Finacle;
    using FintrakBanking.Entities.StagingModels;
    using FintrakBanking.APICore.Filters;
    using FintrakBanking.Interfaces.CRMS;
    using FintrakBanking.Repositories.CRMS;
    using FintrakBanking.APICore.Providers;
    using FintrakBanking.Interfaces.ThridPartyIntegration;
    using FintrakBanking.Repositories.ThirdPartyIntegration;
    using FintrakBanking.Interfaces.AlertMonitoring;
    using FintrakBanking.Repositories.AlertMonitoring;
    using FintrakBanking.Interfaces.Media;
    using FintrakBanking.Repositories.Media;
    using FintrakBanking.Interfaces.credit;
    using FintrakBanking.Repositories.credit;
    using FintrakBanking.APICore.Controllers;
    using FintrakBanking.Entities.AlertReportingModels;
    using FinTrakBanking.ThirdPartyIntegration.HeadOfficeToSub;
    using FintrakBanking.AccessSubsediary;
    using FintrakBanking.Interfaces.External;
    using FintrakBanking.Repositories.External;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                // Support Ninject for dependency injection in WebAPI
                GlobalConfiguration.Configuration.DependencyResolver = new NinjectResolver(kernel);
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }
       
        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IHeadOfficeToSubIntegration>().To<HeadOfficeToSubIntegration>();
            kernel.Bind<FinTrakBankingContext>().To<FinTrakBankingContext>();
            kernel.Bind<FinTrakBankingDocumentsContext>().To<FinTrakBankingDocumentsContext>();
            kernel.Bind<FinTrakBankingStagingContext>().To<FinTrakBankingStagingContext>();
            kernel.Bind<IBulkDisbursementPackageRepository>().To<BulkDisbursementPackageRepository>();
            kernel.Bind<IGeneralSetupRepository>().To<GeneralSetupRepository>();
            kernel.Bind<ICashBackRepository>().To<CashBackRepository>();
            kernel.Bind<IExternalAlertRepository>().To<ExternalAlertRepository>();
            kernel.Bind<IAuthenticationRepository>().To<AuthenticationRepository>();
            kernel.Bind<IAuthorizationRepository>().To<AuthorizationRepository>();
            kernel.Bind<IChartOfAccountRepository>().To<ChartOfAccountRepository>();
            kernel.Bind<IAccountCategoryRepository>().To<AccountCategoryRepository>();
            kernel.Bind<IAccountTypeRepository>().To<AccountTypeRepository>();
            kernel.Bind<ICompanyRepository>().To<CompanyRepository>();
            kernel.Bind<IBranchRepository>().To<BranchRepository>();
            kernel.Bind<ICurrencyRateRepository>().To<CurrencyRateRepository>();
            kernel.Bind<IRiskSetupRepository>().To<RiskSetupRepository>();
            kernel.Bind<IRiskImplementation>().To<RiskImplementation>();
            kernel.Bind<IStaffRepository>().To<StaffRepository>();
            kernel.Bind<IDepartmentRepository>().To<DepartmentRepository>();
            kernel.Bind<IMisInfoRepository>().To<MisInfoRepository>();
            kernel.Bind<ICollateralTypeRepository>().To<CollateralTypeRepository>();
            kernel.Bind<IAccountSensitivityRepository>().To<AccountSensitivityRepository>();
            kernel.Bind<IProductRepository>().To<ProductRepository>();
            kernel.Bind<IJobTitleRepository>().To<JobTitleRepository>();
            kernel.Bind<IStaffRoleRepository>().To<StaffRoleRepository>();
            kernel.Bind<IProductCollateralTypeRepository>().To<ProductCollateralTypeRepository>();
            kernel.Bind<IProductFeeRepository>().To<ProductFeeRepository>();
            kernel.Bind<ICustomerRepository>().To<CustomerRepository>();
            kernel.Bind<ICustomerProductFeeRepository>().To<CustomerProductFeeRepository>();
            kernel.Bind<ICustomerGroupRepository>().To<CustomerGroupRepository>();
            kernel.Bind<IAuditTrailRepository>().To<AuditTrailRepository>();
            kernel.Bind<ICultureHelper>().To<CultureHelper>();
            kernel.Bind<ICasaRepository>().To<CasaRepository>();
            kernel.Bind<IErrorLogRepository>().To<ErrorLogRepository>();
            kernel.Bind<IEmailRepository>().To<EmailRepository>();
            kernel.Bind<IAdminRepository>().To<AdminRepository>();
            kernel.Bind<ILoanCovenantRepository>().To<LoanCovenantRepository>();
            kernel.Bind<ICountryRepository>().To<CountryRepository>();
            kernel.Bind<ICustomerFSCaptionGroupRepository>().To<CustomerFSCaptionGroupRepository>();
            kernel.Bind<ICustomerFSCaptionRepository>().To<CustomerFSCaptionRepository>();
            kernel.Bind<ICustomerFSCaptionDetailRepository>().To<CustomerFSCaptionDetailRepository>();
            kernel.Bind<ICustomFieldsRepository>().To<CustomFieldsRepository>();
            kernel.Bind<ICustomerFSRatioRepository>().To<CustomerFSRatioRepository>();
            kernel.Bind<IChecklistRepository>().To<ChecklistRepository>();
            kernel.Bind<ILoanRepository>().To<LoanRepository>();
            kernel.Bind<ICreditDrawdownRepository>().To<CreditDrawdownRepository>(); 
            kernel.Bind<IApprovalGroupMappingRepository>().To<ApprovalGroupMappingRepository>();
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
            //kernel.Bind<ILimitRepository>().To<LimitRepository>();
            kernel.Bind<IFinanceTransactionRepository>().To<FinanceTransactionRepository>();
            kernel.Bind<ILoanOperationsRepository>().To<LoanOperationsRepository>();
            kernel.Bind<ICreditLimitValidationsRepository>().To<CreditLimitValidationsRepository>();
            kernel.Bind<IPublicHolidayRepository>().To<PublicHolidayRepository>();
            kernel.Bind<IAccreditedConsultantsRepository>().To<AccreditedConsultantsRepository>();
            kernel.Bind<IReportRoutes>().To<ReportRoutes>();
            kernel.Bind<ICallMemoRepository>().To<CallMemoRepository>();
            kernel.Bind<IConditionPrecedentRepository>().To<ConditionPrecedentRepository>();
            kernel.Bind<ITransactionDynamicsRepository>().To<TransactionDynamicsRepository>();
            kernel.Bind<IEmailAndAlertsRepository>().To<EmailAndAlertsRepository>();
            kernel.Bind<IEndOfDayRepository>().To<EndOfDayRepository>();
            kernel.Bind<IFinanceTransactionsReport>().To<FinanceTransactionsReport>();
            kernel.Bind<IKYCDocumentUploadRepository>().To<KYCDocumentUploadRepository>();
            kernel.Bind<IMonitoringSetupRepository>().To<MonitoringSetupRepository>();
            kernel.Bind<IOfferLetterAndAvailmentRepository>().To<OfferLetterAndAvailmentRepository>();
            kernel.Bind<ICustomerStagingRepository>().To<CustomerStagingRepository>();
            kernel.Bind<ILoanPrincipalRepository>().To<LoanPrincipalRepository>();
            kernel.Bind<ILoanMarketRepository>().To<LoanMarketRepository>();
            kernel.Bind<IPrudentialGuidelineSetupRepository>().To<PrudentialGuidelineSetupRepository>();
            kernel.Bind<ILoanRecoverySetupRepository>().To<LoanRecoverySetupRepository>();
            kernel.Bind<IEmployerRepository>().To<EmployerRepository>();
            kernel.Bind<ILoanReviewApplicationRepository>().To<LoanReviewApplicationRepository>();
            kernel.Bind<ICustomerCreditBureauRepository>().To<CustomerCreditBureauRepository>();
            kernel.Bind<IFeeConcessionRepository>().To<FeeConcessionRepository>();
            kernel.Bind<ILimitAndMonitoringRepository>().To<LimitAndMonitoringRepository>();
            kernel.Bind<IApprovalReliefRepository>().To<ApprovalReliefRepository>();
            kernel.Bind<ICasaLienRepository>().To<CasaLienRepository>();
            kernel.Bind<IContingentLoanUsageRepository>().To<ContingentLoanUsageRepository>();
            kernel.Bind<IOverRideRepository>().To<OverRideRepository>();
            kernel.Bind<IStaffAccountHistoryRepository>().To<StaffAccountHistoryRepository>();
            kernel.Bind<IIntegrationWithFinacle>().To<IntegrationWithFlexcube>();
            kernel.Bind<ILoanPerformanceRepository>().To<LoanPerformanceRepository>();
            kernel.Bind<IOverDraftValidation>().To<OverDraftValidation>();
            kernel.Bind<ICustomChartOfAccountRepository>().To<CustomChartOfAccountRepository>();
            kernel.Bind<IProfileSetupRepository>().To<ProfileSetupRepository>();
            kernel.Bind<IFXAccountCreationRepository>().To<FXAccountCreationRepository>();
            kernel.Bind<ILaonCamSolRepository>().To<LaonCamSolRepository>();
            kernel.Bind<ITwoFactorAuthIntegrationService>().To<TwoFactorAuthIntegrationService>();
            kernel.Bind<IStaffMIS>().To<StaffMIS>();
            kernel.Bind<IFacilityDetailSummary>().To<FacilityDetailSummary>();
            kernel.Bind<ICRMSRegulatories>().To<CRMSRegulatories>();
            kernel.Bind<IMemorandumRepository>().To<MemorandumRepository>();
            kernel.Bind<ICrmsRegulatoryRepository>().To<CrmsRegulatoryRepository>();
            kernel.Bind<IDashboardRepository>().To<DashboardRepository>();
            kernel.Bind<ICRMSCodeBookRepository>().To<CRMSCodeBookRepository>();
            kernel.Bind<IAPIErrorLog>().To<APIErrorLog>();
            kernel.Bind<ApplicationOAuthProvider>().To<ApplicationOAuthProvider>();
            kernel.Bind<CreditCommonRepository>().To<CreditCommonRepository>();
            kernel.Bind<ILoanFeeChargeRepository>().To<LoanFeeChargeRepository>();
            kernel.Bind<ISupportUtilityRepository>().To<SupportUtilityRepository>();

            kernel.Bind<IFinacleIntegrationRepository>().To<FinacleIntegrationRepository>();
            kernel.Bind<IEmailAlertLogger>().To<EmailAlertLogger>();
            kernel.Bind<IBusinessRuleRepository>().To<BusinessRuleRepository>();
            kernel.Bind<IDocumentUploadRepository>().To<DocumentUploadRepository>();
            kernel.Bind<IDocumentCategoryRepository>().To<DocumentCategoryRepository>();
            kernel.Bind<IDocumentCategoryTypeRepository>().To<DocumentCategoryTypeRepository>();
            kernel.Bind<IDocumentTypeRepository>().To<DocumentTypeRepository>();
            kernel.Bind<IDocumentUsageRepository>().To<DocumentUsageRepository>();
            kernel.Bind<IRiskAcceptanceCriteriaRepository>().To<RiskAcceptanceCriteriaRepository>();
            kernel.Bind<ITermSheetRepository>().To<TermSheetRepository>();
            kernel.Bind<ICreditOfficerRiskRepository>().To<CreditOfficerRiskRepository>();
            kernel.Bind<IOriginalDocumentApprovalRepository>().To<OriginalDocumentApprovalRepository>();
            kernel.Bind<ILcIssuanceRepository>().To<LcIssuanceRepository>();
            kernel.Bind<ILcDocumentRepository>().To<LcDocumentRepository>();
            kernel.Bind<ILcShippingRepository>().To<LcShippingRepository>();
            kernel.Bind<ILcConditionRepository>().To<LcConditionRepository>();
            kernel.Bind<ICustomerCollateralRepository>().To<CustomerCollateralRepository>();

            kernel.Bind<IAtcLodgmentRepository>().To<AtcLodgmentRepository>();
            kernel.Bind<IAtcLodgmentDetailRepository>().To<AtcLodgmentDetailRepository>();
            kernel.Bind<IProjectSiteReportRepository>().To<ProjectSiteReportRepository>();
            kernel.Bind<ILcUssanceRepository>().To<LcUssanceRepository>();
            kernel.Bind<ILetterGenerationRequestRepository>().To<LetterGenerationRequestRepository>();
            kernel.Bind<IValuationReportRepository>().To<ValuationReportRepository>();
            kernel.Bind<ICollateralValuationRepository>().To<CollateralValuationRepository>();
            kernel.Bind<IValuationRequestTypeRepository>().To<ValuationRequestTypeRepository>();
            kernel.Bind<IOriginalDocumentReleaseRepository>().To<OriginalDocumentReleaseRepository>();
            kernel.Bind<IRepaymentTermsRepository>().To<RepaymentTermsRepository>();
            kernel.Bind<IAuthourisedSignatoryRepository>().To<AuthourisedSignatoryRepository>();
            kernel.Bind<IAlertRepository>().To<AlertRepository>();
            kernel.Bind<ICashFlowLendingRepository>().To<CashFlowLendingRepository>();
            kernel.Bind<ILoanArchiveRepository>().To<LoanArchiveRepository>();
            kernel.Bind<IFacilityModificationRepository>().To<FacilityModificationRepository>();
            kernel.Bind<ILcCashBuildUpPlanRepository>().To<LcCashBuildUpPlanRepository>();
            kernel.Bind<ISubsediaryParentController>().To<SubsediaryParentController>();
            kernel.Bind<IDigitalStampRepository>().To<DigitalStampRepository>();
            kernel.Bind<IExposureRepository>().To<ExposureRepository>();
            kernel.Bind<ICustomerRepositoryExternal>().To<CustomerRepositoryExternal>();
            kernel.Bind<IGeneralRepositoryExternal>().To<GeneralRepositoryExternal>();
            kernel.Bind<ILoanRepositoryExternal>().To<LoanRepositoryExternal>();
        }
    }
    
}
