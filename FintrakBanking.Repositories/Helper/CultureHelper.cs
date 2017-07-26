using FintrakBanking.Common;
using FintrakBanking.Interfaces.Helper; 
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Text;

namespace FintrakBanking.Repositories.Helper
{
    [Export(typeof(ICultureHelper))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CultureHelper : ICultureHelper
    {
        

        public DateTime DateTimeToCurrentCulture(string input)
        {
            string convertedDate = string.Empty;
            var currInfo = CultureInfo.CurrentCulture.Name;
            var inputedCurrInfo =CommonHelpers. CultureInfo;
            DateTimeFormatInfo dInfo = new CultureInfo(currInfo).DateTimeFormat;
            DateTimeFormatInfo britainInfo = new CultureInfo(inputedCurrInfo).DateTimeFormat;

            if (currInfo == inputedCurrInfo)
            {
                return Convert.ToDateTime(dInfo.ShortDatePattern);
            }
            convertedDate = Convert.ToDateTime(input, britainInfo).ToString(dInfo.ShortDatePattern);
            return Convert.ToDateTime(convertedDate);
        }
    }
}
