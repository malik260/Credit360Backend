
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FintrakBanking.Common
{
    public static class CommonHelpers
    {
        /// <summary>
        /// Format DateTime value for easy insertion into Database.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string DateTimeToString(DateTime date)
        {
            return String.Format("{0:dd-MMM-yyyy}", date);
        }

        /// <summary>
        /// For parsing DateTime string from the format "yyyyMMdd"
        /// </summary>
        /// <param name="dtString">DateTime string in "yyyyMMdd" format</param>
        /// <returns></returns>
        public static DateTime ParseDate(string dtString)
        {
            return DateTime.ParseExact(dtString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// If the current exception is just a wrapper for InnerException, clips the Wrapper exception to 50 characters
        /// If InnerException is not null. Clip the current exception to 50 characters.  
        /// E.g. "Current exception Message...{upto 50characters} + "...->" + InnerException.Message
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FormatException(Exception ex)
        {
            var message = ex.Message;

            //Add the inner exception if present (showing only the first 50 characters of the first exception)
            if (ex.InnerException == null) return message;
            if (message.Length > 50)
                message = message.Substring(0, 50);

            message += "...->" + ex.InnerException.Message;

            return message;
        }

        public static string FormatToNaira(decimal value)
        {
            var retVal = string.Format("{0:0,0.00}", value);
            return "=N=" + retVal;
        }

        public static string FormatNumber(decimal value, int decimalPlaces)
        {
            var retVal = string.Empty;
            if (decimalPlaces == 1)
            {
                retVal = string.Format("{0:0,0.0}", value);
            }
            else if (decimalPlaces == 2)
            {
                retVal = string.Format("{0:0,0.00}", value);
            }

            return retVal;
        }

        public static string FormatNumberTwoPlaces(decimal value)
        {
            return string.Format("{0:0,0.00}", value); 
        }


        /// <summary>
        /// Ensures the subscriber email or throw.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public static string EnsureSubscriberEmailOrThrow(string email)
        {
            string output = EnsureNotNull(email);
            output = output.Trim();
            output = EnsureMaximumLength(output, 255);

            if (!IsValidEmail(output))
            {
                //throw new SmartException("Email is not valid.");
            }

            return output;
        }

        /// <summary>
        /// Verifies that a string is in valid e-mail format
        /// </summary>
        /// <param name="email">Email to verify</param>
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        public static bool IsValidEmail(string email)
        {
            bool result = false;
            if (String.IsNullOrEmpty(email))
                return result;
            email = email.Trim();
            result = Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            return result;
        }

        /// <summary>
        /// Generate random digit code
        /// </summary>
        /// <param name="length">Length</param>
        /// <returns>Result string</returns>
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }



        /// <summary>
        /// Ensure that a string doesn't exceed maximum allowed length
        /// </summary>
        /// <param name="str">Input string</param>
        /// <param name="maxLength">Maximum length</param>
        /// <returns>Input string if its lengh is OK; otherwise, truncated input string</returns>
        public static string EnsureMaximumLength(string str, int maxLength)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Length > maxLength)
                return str.Substring(0, maxLength);
            return str;
        }

        /// <summary>
        /// Ensures that a string only contains numeric values
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Input string with only numeric values, empty string if input is null/empty</returns>
        public static string EnsureNumericOnly(string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            var result = new StringBuilder();
            foreach (char c in str)
            {
                if (Char.IsDigit(c))
                    result.Append(c);
            }
            return result.ToString();
        }


        public static bool IsNumeric(string value)
        {
            bool isNum = false;
            Int32 output;
            if (Int32.TryParse(value, out output))
            {
                isNum = true;
            }

            return isNum;
        }

        public static bool IsDecimal(string value)
        {
            bool isNum = false;
            decimal output;
            if (decimal.TryParse(value, out output))
            {
                isNum = true;
            }

            return isNum;
        }

        /// <summary>
        /// Ensure that a string is not null
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Result</returns>
        public static string EnsureNotNull(string str)
        {
            if (str == null)
                return string.Empty;

            return str;
        }



        public static bool IsAllowedImageExtension(string fileExt)
        {
            bool isValid = false;

            string[] allowExt = { ".jpg", ".png", ".jpeg", ".bmp" };//Add to the array if desired

            for (int i = 0; i <= allowExt.Length - 1; i++)
            {
                if (fileExt == allowExt[i])
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }



        /// <summary>
        /// Get substring of specified number of characters on the right.
        /// </summary>
        public static string Right(this string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(value.Length - length);
        }

        /// <summary>
        /// Get substring of specified number of characters on the left.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string Left(this string value, int length)
        {
            if (String.IsNullOrEmpty(value)) return string.Empty;

            return value.Length <= length ? value : value.Substring(0, length);
        }

        public static string GenerateZeroString(int length)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append("0");
            }
            return sb.ToString();
        }

        public static string AppendZeroString(int value, int maxLength)
        {
            string sb = string.Empty;
            for (int i = value.ToString().Length; i < maxLength; i++)
            {
                sb += "0";
            }
            string result = sb + value.ToString();
            return result;
        }

        public static int PasswordExpirationDays
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["PasswordExpiredDays"]);
            }
        }

        public static string DomanProvider
        {
            get
            {
                return ConfigurationManager.AppSettings["DomainName"];
            }
        }

        public static string ReportPath
        {
            get
            {
                return ConfigurationManager.AppSettings["reportPath"];
            }
        }

        public static string SendErrorMail
        {
            get
            {
                return (ConfigurationManager.AppSettings["sendErrorMail"]).ToString();
            }
        }


        public static string SmtpClientMail
        {
            get
            {
                return (ConfigurationManager.AppSettings["smtpClient"]).ToString();
            }
        }
        public static string SmtpPort
        {
            get
            {
                return (ConfigurationManager.AppSettings["smtpPort"]).ToString();
            }
        }

        public static string CultureInfo
        {
            get
            {
                return (ConfigurationManager.AppSettings["British"]).ToString();
            }
        }

        public static int MaxInvalidPasswordAttempts
        {
            get
            {
                return  int.Parse( (ConfigurationManager.AppSettings["maxInvalidPasswordAttempts"]).ToString());
            }
        }

        public static string minRequiredPasswordLength
        {
            get
            {
                return (ConfigurationManager.AppSettings["minRequiredPasswordLength"]).ToString();
            }
        }

        public static string minRequiredNonalphanumericCharacters
        {
            get
            {
                return (ConfigurationManager.AppSettings["minRequiredNonalphanumericCharacters"]).ToString();
            }
        }

        public static string GetUserIP()
        {
            var ip = (!String.IsNullOrWhiteSpace(HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]))
                     ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"]
                     : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (ip.Contains(","))
                ip = ip.Split(',').First().Trim();
            return ip;
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[62];
            chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            string code = string.Empty;
            for (int i = 0; i < result.Length; i++)
            {
                code += result[i];
            }
            return code;
        }


        public static int GetLoanReferanceNumber()
        {
            //var salt = GetUniqueKey(4).ToUpper();

            TimeSpan epochTicks = new TimeSpan(new DateTime(1980, 1, 1).Ticks);
            TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            double unixTime = unixTicks.TotalSeconds;

            //var output = salt + unixTime.ToString();

            return (int)unixTime;
        }


        public static byte[] ToByteArray(this string str)
        {
            return System.Text.Encoding.ASCII.GetBytes(str);
        }

        public static string FileToBase64(this string imgPath)
        {
            byte[] imageBytes = System.IO.File.ReadAllBytes(imgPath);
            string base64String = Convert.ToBase64String(imageBytes);
            return base64String;
        }
        public static byte[] Base64ToByte(this string base64String)
        {
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            return byteBuffer;
        }
        public static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                        (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
