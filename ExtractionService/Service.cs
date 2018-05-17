 
    using System; 
    using Microsoft.Owin.Hosting;
    using Ninject;
    using Topshelf;


    namespace ExtractionService
    {
        using Configs;
    using ExtractionService.Contracts;
    using System.Threading.Tasks;

    public class Service
        {
            private readonly IKernel kernel;
        private readonly IExtraction extract;
        // inject the kernel into the Service class for later use
        public Service(IKernel kernel, IExtraction extract)
            : base()
        {
            this.kernel = kernel;
            this.extract = extract;
        }

            protected IKernel Kernel
            {
                get
                {
                    return this.kernel;
                }
            }

            protected IDisposable WebAppHolder
            {
                get;
                set;
            }

            protected int Port
            {
                get
                {
                    return 9000;
                }
            }

            public bool Start(HostControl hostControl)
            {
                if (WebAppHolder == null)
                {
                    // don't use the OwinStartupAttribute to bootstrap OWIN
                    //
                    // provide an Action<IAppBiulder> action to
                    // the WebApp.Start instead; this action will be
                    // called at start up
                    //
                    // adjust the signature of the Configure method to take
                    // an IKernel instance in addition to
                    // an IAppBuilder instance
                    //
                    // use this instance of the pre-instantiated kernel
                    // passed in to the Service class (and subsequently to
                    // the StartupConfig.Configure method) to plug in as the
                    // middleware
                    WebAppHolder = WebApp.Start
                    (
                        new StartOptions
                        {
                            Port = Port
                        },
                        appBuilder =>
                        {
                            new StartupConfig().Configure(appBuilder, Kernel);

                          Task.Run(() =>   extract.CurrencyExchangeRateExtraction());
                            //extract.CustomerAccountBalances();
                            //extract.CustomerAccountExtraction();
                            //extract.ProductPricingExtraction();
                        } 
                        
                         
                    );
                }

                return true;
            }

            public bool Stop(HostControl hostControl)
            {
                if (WebAppHolder != null)
                {
                    WebAppHolder.Dispose();
                    WebAppHolder = null;
                }

                return true;
            }
        }
    }
 