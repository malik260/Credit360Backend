namespace ExtractionService
{
    using Contracts;

    namespace Services
    {
        public class DefaultStringProvider : IStringProvider
        {
            public static int Count = 0;

            public string HelloWorld
            {
                get
                {
                    return string.Format("Hello World #{0}!", ++Count);
                }
            }
        }
    }
}
//08140582033