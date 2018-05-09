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
                    // Environment.CurrentDirectory + 
                    string FILE_NAME = @"C:\FinTrakLog\ServiceLog_File.txt";
                    if (!File.Exists(FILE_NAME))
                    {
                        FileStream fs = File.Create(FILE_NAME);
                        fs.Close();
                    }
                    StreamWriter writer = File.AppendText(FILE_NAME);
                    try
                    {
                        writer.WriteLine();
                        writer.WriteLine(logString + Environment.NewLine);
                        writer.Flush();
                        writer.Close();
                    }
                    catch (Exception )
                    {
                        writer.Close();
                    }
                }
                catch (Exception )
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
    }
}
