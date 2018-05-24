using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FinTrakMail
{
   public class AuditTrail
    {
        public class LogFileManager
        {
            private static string _servicename;
            public string LogFolder;
            public string LogPath;

            public static void LogToFile(string logString)
            {
                try
                {
                    LogService(logString);

                }
                catch (Exception ex)
                {
                }
            }

            public static string ServiceName
            {
                get { return _servicename; }
                private set { _servicename = value; }
            }

            public static void GetServiceName(string SvrName)
            {
                _servicename = SvrName;
            }
        }

        private static void LogService(string content)
        {
            string FILE_NAME = @"C:\Fintrak360EmailService_Log\ServiceLog_File.txt";
            //bool exists = Directory.Exists(FILE_NAME);

            //if (!exists)
            //    Directory.CreateDirectory(FILE_NAME);

            FileStream fs = new FileStream(FILE_NAME, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.BaseStream.Seek(0, SeekOrigin.End);
            sw.WriteLine(content);
            sw.WriteLine(" ");
            sw.Flush();
            sw.Close();
        }
    }
}
