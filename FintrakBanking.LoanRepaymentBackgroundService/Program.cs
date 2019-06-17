using FintrakBanking.Interfaces.Credit;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.Ninject;

namespace FintrakBanking.LoanRepaymentBackgroundService
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger logger = LogManager.GetCurrentClassLogger();
            try
            {

                logger.Error("");
                logger.Error("==================================================================");
                logger.Error("DI resolving started at : " + DateTime.Now);

                var kernel = new StandardKernel();
                kernel.Load(Assembly.GetExecutingAssembly());
                var detail = kernel.Get<ILoanOperationsRepository>();

                logger.Error("");
                logger.Error("==================================================================");
                logger.Error("DI resolving ended at : " + DateTime.Now);


                logger.Error("");
                logger.Error("==================================================================");
                logger.Error("Loan repayment has started successfully at : " + DateTime.Now);

                detail.GetRepaymentFromStaging();

                logger.Error("");
                logger.Error("==================================================================");
                logger.Error("Loan repayment has ended successfully at : " + DateTime.Now);

            }
            catch (Exception ex)
            {

                logger.Error("Faled Error Log");
                logger.Error("==================================================================");
                logger.Error(ex.Message + "  ------    " + DateTime.Now);
                logger.Error("==================================================================");
                logger.Error(ex.InnerException + "  ------    " + DateTime.Now);
                logger.Error("==================================================================");
                logger.Error(ex.StackTrace + "  ------    " + DateTime.Now);
                logger.Error("==================================================================");
            }
        }
    }
}
