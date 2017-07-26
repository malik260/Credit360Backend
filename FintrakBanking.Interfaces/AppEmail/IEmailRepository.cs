 

namespace FintrakBanking.Interfaces.AppEmail
{
    public interface IEmailRepository
    {
        
        void sendMail(string to, string from, string cc, string bcc, string subject, string message);
        
    }
}