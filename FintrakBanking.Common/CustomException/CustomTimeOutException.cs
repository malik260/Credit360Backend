using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.CustomException
{
    public class CustomTimeoutException : TimeoutException
    {
        public CustomTimeoutException(string literal): base(String.Format(literal)) { }
    }
}
