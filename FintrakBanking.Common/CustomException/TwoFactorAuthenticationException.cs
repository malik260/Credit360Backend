using System;

namespace FintrakBanking.Common.CustomException
{
    [Serializable]
    public class TwoFactorAuthenticationException : SecureException
    {
        public TwoFactorAuthenticationException(string literal) : base(String.Format(literal)) { }
    }
}
