using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.CustomException
{
    public class SecureException : Exception
    {
        public SecureException(string message) : base(message) { }
        public SecureException(string message, Exception ex) : base(message, ex) { }
    }
}
