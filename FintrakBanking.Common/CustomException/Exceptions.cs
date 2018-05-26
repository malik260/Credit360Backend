using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.CustomException
{
    //class Exceptions
    //{
    //}
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
}
