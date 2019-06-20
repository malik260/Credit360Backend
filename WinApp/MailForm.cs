using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
//using TextmagicRest;
//using TextmagicRest.Model;
using System.Net.Http;


namespace WinApp
{
    public partial class MailForm : Form
    {
        public MailForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            foreach (string filePath in openFileDialog1.FileNames)
            {
                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    lblAttachments.Text += fileName + Environment.NewLine;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            using (MailMessage mm = new MailMessage(txtsendermail.Text.Trim(), txtTo.Text.Trim()))
            {
                mm.Subject = txtsendermail.Text;
                mm.Body = txtbody.Text;
                foreach (string filePath in openFileDialog1.FileNames)
                {
                    if (File.Exists(filePath))
                    {
                        string fileName = Path.GetFileName(filePath);
                        mm.Attachments.Add(new Attachment(filePath));
                    }
                }
                mm.IsBodyHtml = false;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.EnableSsl = true;
                NetworkCredential NetworkCred = new NetworkCredential(txtsendermail.Text.Trim(), txtpassword.Text.Trim());
                smtp.UseDefaultCredentials = true;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mm);
                MessageBox.Show("Email sent.", "Message");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //  var client = new Client("test", "oxlUazdcSpiJTnlHAc42rA");
            //  var link = client.SendMessage("Hello from TextMagic API", "2348062417211");
            //  if (link.Success)
            //  {
            //      Console.WriteLine("Message ID {0} has been successfully sent", link.Id);
            //  }
            //  else
            //  {
            //      Console.WriteLine("Message was not sent due to following exception: {0}", link.ClientException.Message);
            //}
            var client2 = new HttpClient();
            //client2.GetAsync("https://platform.clickatell.com/messages/http/send?apiKey=oxlUazdcSpiJTnlHAc42rA==&to=2348062417211&content=This+is+from+Gbenga");
            client2.GetAsync("https://sms.bbnplace.com/bulksms/bulksms.php?username=tayoomoemma@gmail.com&password=wine123&sender=Gbenga&message=This+is+from+Gbenga&mobile=07032424623");
            MessageBox.Show("SMS SEND!");
        }
    }
}
