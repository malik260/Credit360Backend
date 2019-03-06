[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(FintrakBanking.BackgroundTasksService.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(FintrakBanking.BackgroundTasksService.App_Start.NinjectWebCommon), "Stop")]

namespace FintrakBanking.BackgroundTasksService.App_Start
{
    using System;
    using System.Web;
    using FintrakBanking.Entities.DocumentModels;
    using FintrakBanking.Entities.Models;
    using FintrakBanking.Entities.StagingModels;
    using FintrakBanking.Interfaces.AlertMonitoring;
    using FintrakBanking.Repositories.AlertMonitoring;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;

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
            kernel.Bind<FinTrakBankingContext>().To<FinTrakBankingContext>();
            kernel.Bind<FinTrakBankingDocumentsContext>().To<FinTrakBankingDocumentsContext>();
            kernel.Bind<FinTrakBankingStagingContext>().To<FinTrakBankingStagingContext>();
            kernel.Bind<IEmailSender>().To<EmailSender>();
            kernel.Bind<IAlertMessageLogger>().To<AlertMessageLogger>();
            kernel.Bind<ISLANotification>().To<SLANotification>();
            kernel.Bind<IAlertMessagesEngine>().To<AlertMessagesEngine>();

        }
    }
}