using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Notification.API.Services
{
    public interface IEmailService
    {
        Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string fileName);
    }

    public class EmailService : IEmailService
    {
        public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, byte[] attachment, string fileName)
        {
            Console.WriteLine($"Simulating sending email to {to} with attachment {fileName}...");
            
            // Real implementation would look like this:
            /*
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Ticket Booking", "noreply@ticketbooking.com"));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var builder = new BodyBuilder { TextBody = body };
            builder.Attachments.Add(fileName, attachment);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.mailtrap.io", 587, false);
            await client.AuthenticateAsync("user", "pass");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            */
            await Task.Delay(500);
            Console.WriteLine("Email sent successfully (simulated).");
        }
    }
}
