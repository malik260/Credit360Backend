using FintrakBanking.Interfaces.Credit;
using System;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {
        [Serializable]
        public class CustomException : Exception
        {
            public CustomException() { }
            public CustomException(string message) : base(message) { }
            public CustomException(string message, Exception inner) : base(message, inner) { }
            protected CustomException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}

