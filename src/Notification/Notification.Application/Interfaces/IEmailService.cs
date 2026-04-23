using System.Threading.Tasks;

namespace Notification.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string fileName);
    }
}
