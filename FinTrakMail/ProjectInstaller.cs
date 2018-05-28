using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;

namespace FinTrakMail
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller serviceProcessInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();
            MailProcessInstaller.Parent = this;
            FintrakEmailSenderService.Parent = this;
        }

        private void MailserviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            new ServiceController(FintrakEmailSenderService.ServiceName).Start();
        }

        private void MailProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            serviceInstaller = new ServiceInstaller();
            serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "FintrakMail";
            serviceInstaller.DisplayName = "Fintrak Credit 360 E-Mail Sender";
            serviceInstaller.Description = "Escalte Transaction Emails";
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            Installers.Add(serviceInstaller);

            
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;
            Installers.Add(serviceProcessInstaller);

        }
    }
}
