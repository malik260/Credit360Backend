using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            CompositionRoot.Wire(new ApplicationModule());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //IAuditTrailRepository _auditTrail = new Form1();
            FinTrakBankingContext _context = new FinTrakBankingContext();
            //Application.Run(new Form1(_context));
            Application.Run(CompositionRoot.Resolve<Form1>());
            //Application.Run(CompositionRoot.Resolve<MailForm>());
        }
    }
}
