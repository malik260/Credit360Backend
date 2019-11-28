using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.MessagingAlertSender
{
    public class LogError
    {
        private DateTime errorDt;
        private string src;
        private Exception errorInfo;
        public static string strDirectoryPath;
        public DateTime ErrorDate
        {
            get { return errorDt; }
            set { errorDt = value; }
        }
        public string ErrorSrc
        {
            get { return src; }
            set { src = value; }
        }
        public Exception ErrorInformation
        {
            get { return errorInfo; }
            set { errorInfo = value; }
        }

        public static void Log_Err(string strErrorSource, Exception Ex)
        {
            LogError errInfo = new LogError();
            errInfo.ErrorDate = System.DateTime.Now;
            errInfo.ErrorSrc = strErrorSource;
            errInfo.ErrorInformation = Ex;
            LogError.LogErr(errInfo);
        }


        public static void LogErr(LogError errorDTO)
        {
            try
            {
                string directoryPath = strDirectoryPath;
                if (!string.IsNullOrEmpty(strDirectoryPath))
                {
                    string path = directoryPath + "\\" + "ErrorLog.txt";
                    StreamWriter swErrorLog = null;
                    DirectoryInfo dtDirectory = null;
                    if (!Directory.Exists(directoryPath))
                    {
                        dtDirectory = Directory.CreateDirectory(directoryPath);
                        dtDirectory = null;
                    }
                    if (File.Exists(path))
                    {
                        swErrorLog = new StreamWriter(path, true); //append the error message
                        swErrorLog.WriteLine("Date and Time of Exception: " + errorDTO.ErrorDate);
                        swErrorLog.WriteLine("Source of Exception: " + errorDTO.ErrorSrc);
                        swErrorLog.WriteLine(" ");
                        swErrorLog.WriteLine("Error Message: " + errorDTO.ErrorInformation);
                        swErrorLog.WriteLine("------------------------------------------- ");
                        swErrorLog.WriteLine(" ");
                        //swErrorLog.WriteLine(System.Security.Principal.WindowsIdentity.GetCurrent().Name);
                        swErrorLog.Close();
                        swErrorLog = null;
                    }
                    else
                    {
                        swErrorLog = File.CreateText(path);
                        swErrorLog = new StreamWriter(path, true); //append the error message
                        swErrorLog.WriteLine("Date and Time of Exception: " + errorDTO.ErrorDate);
                        swErrorLog.WriteLine("Source of Exception: " + errorDTO.ErrorSrc);
                        swErrorLog.WriteLine(" ");
                        swErrorLog.WriteLine("Error Message: " + errorDTO.ErrorInformation);
                        swErrorLog.WriteLine("------------------------------------------- ");
                        swErrorLog.WriteLine(" ");
                        swErrorLog.Close();
                        swErrorLog = null;
                    }
                }
            }
            catch (NullReferenceException)
            {
                throw;
            }
        }
    }
}