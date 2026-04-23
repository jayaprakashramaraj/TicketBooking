using System;
using System.Threading.Tasks;
using Notification.Application.Interfaces;

namespace Notification.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string fileName)
        {
            Console.WriteLine($"Simulating sending email to {to} with attachment {fileName}...");
            await Task.Delay(500);
            Console.WriteLine("Email sent successfully (simulated).");
        }
    }
}
