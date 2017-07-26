using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Helper
{
    public interface ICultureHelper
    {
        DateTime DateTimeToCurrentCulture(string input);
    }
}
