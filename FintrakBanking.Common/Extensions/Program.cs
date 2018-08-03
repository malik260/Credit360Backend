using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Extensions
{
    class Program
    {
        static void Main(string[] args)
        {
            var dateFormat = "ddMMyyyyHHmmss";
            var date = DateTime.Now;
            string dateInfo = date.ToString(dateFormat);
            //var dateInfo = date; //.Replace("/", "").Replace(":", "").Replace(" ", "");
            //var salt = "devFin59"; //2

            //dateInfo = "44201885635PM";
            HashHelper hashValue = new HashHelper();

            var hashedValue = hashValue.HashString(dateInfo).Replace("-", "");

            var hash = hashedValue; // hashedPassword.Replace("-", "");

            DateTime incomingDate = DateTime.ParseExact(dateInfo, dateFormat, CultureInfo.InvariantCulture);

            string incomingDateInfo = incomingDate.ToString(dateFormat);

            var currentDate = DateTime.Now;

            var dateDifference = currentDate - incomingDate;

            var message = "";

            if (dateDifference.Seconds > 20)
                message = "t: report access denied";

            if (dateInfo == incomingDateInfo)
                message = "Okay";

        }
    }
}
