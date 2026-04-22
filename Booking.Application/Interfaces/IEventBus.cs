using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Booking.Application.Interfaces
{
    public interface IEventBus
    {
        Task PublishAsync<T>(T message) where T : class;
    }
}
