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
            this.FintrakEmailSenderService = new System.ServiceProcess.ServiceInstaller();
            // 
            // MailProcessInstaller
            // 
            this.MailProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.MailProcessInstaller.Password = null;
            this.MailProcessInstaller.Username = null;
            this.MailProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MailProcessInstaller_AfterInstall);
            // 
            // FintrakEmailSenderService
            // 
            this.FintrakEmailSenderService.Description = "FinTrak E-Mail Service";
            this.FintrakEmailSenderService.DisplayName = "FinTrak E-Mail Service";
            this.FintrakEmailSenderService.ServiceName = "Fintrak Email Sender";
            this.FintrakEmailSenderService.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.FintrakEmailSenderService.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.MailserviceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.MailProcessInstaller,
            this.FintrakEmailSenderService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller MailProcessInstaller;
        private System.ServiceProcess.ServiceInstaller FintrakEmailSenderService;
    }
}