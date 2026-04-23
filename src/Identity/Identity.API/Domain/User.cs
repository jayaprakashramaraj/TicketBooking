using System;

namespace Identity.API.Domain
{
    public class User
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string FullName { get; set; }
        public required string Role { get; set; } // "Admin" or "User"
    }
}
