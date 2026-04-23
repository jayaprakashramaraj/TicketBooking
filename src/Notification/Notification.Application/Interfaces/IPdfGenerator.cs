using System;
using System.Collections.Generic;

namespace Notification.Application.Interfaces
{
    public interface IPdfGenerator
    {
        byte[] GenerateTicket(string customerEmail, string showName, DateTime showTime, List<string> seats, decimal amount);
    }
}
