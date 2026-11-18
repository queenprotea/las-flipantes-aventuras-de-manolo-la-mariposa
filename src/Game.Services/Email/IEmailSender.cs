using System.Threading.Tasks;

namespace Game.Services
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(EmailMessage message);
    }
}
