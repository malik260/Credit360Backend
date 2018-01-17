namespace FinTrakMail
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MailProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.MailserviceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // MailProcessInstaller
            // 
            this.MailProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MailProcessInstaller.Password = null;
            this.MailProcessInstaller.Username = null;
            // 
            // MailserviceInstaller
            // 
            this.MailserviceInstaller.Description = "FinTrak E-Mail Service";
            this.MailserviceInstaller.DisplayName = "FinTrak E-Mail Service";
            this.MailserviceInstaller.ServiceName = "MailServer";
            this.MailserviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.MailserviceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MailserviceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MailProcessInstaller,
            this.MailserviceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MailProcessInstaller;
        private System.ServiceProcess.ServiceInstaller MailserviceInstaller;
    }
}