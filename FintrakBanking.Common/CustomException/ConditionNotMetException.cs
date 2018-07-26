using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.CustomException
{
    [Serializable]
    public class ConditionNotMetException : SecureException
    {
        public ConditionNotMetException(string literal): base(String.Format(literal)) { }
    }
}
