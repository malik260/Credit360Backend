using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
//using System.Web.Hosting;

namespace EmailMessageLogger
{
    public class EmailHelpers
    {
        public static string PopulateBody(string description, string templateLink)
        {
          
            string path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName, templateLink);

            string body;

            using (var reader = new StreamReader(path ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{Description}", description);
            return body;
        }

    }

    public class EmailFormViewModel
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
    }
}