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
        public ProjectInstaller()
        {
            InitializeComponent();
            MailProcessInstaller.Parent = this;
            MailserviceInstaller.Parent = this;
        }

        private void MailserviceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            new ServiceController(MailserviceInstaller.ServiceName).Start();
        }
    }
}
