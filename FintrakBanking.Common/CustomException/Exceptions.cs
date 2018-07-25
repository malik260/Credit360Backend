using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.CustomException
{
    [Serializable]
    public class ConditionNotMetException : Exception
    {
        public ConditionNotMetException()
        {

        }

        public ConditionNotMetException(string literal)
        : base(String.Format(literal))
        {

        }
    }

    public class BadLogicException : Exception
    {
        public BadLogicException()
        {

        }

        public BadLogicException(string literal)
        : base(String.Format(literal))
        {

        }
    }

    public class APIErrorException : Exception
    {
        public APIErrorException()
        {

        }

        public APIErrorException(string literal)
        : base(String.Format(literal))
        {

        }
    }

    public class TwoFactorAuthenticationException : Exception
    {
        public TwoFactorAuthenticationException()
        {

        }

        public TwoFactorAuthenticationException(string literal)
        : base(String.Format(literal))
        {

        }
    }

}
