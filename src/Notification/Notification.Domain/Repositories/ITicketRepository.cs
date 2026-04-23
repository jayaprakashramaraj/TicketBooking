using System;
using System.Threading.Tasks;

namespace Notification.Domain.Repositories
{
    public interface ITicketRepository
    {
        Task SaveTicketAsync(Guid bookingId, byte[] pdfContent);
        Task<byte[]?> GetTicketAsync(Guid bookingId);
    }
}
