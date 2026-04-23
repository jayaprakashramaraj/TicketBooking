using System;

namespace Identity.Application.DTOs
{
    public record RegisterRequest(string Email, string Password, string FullName);
    public record LoginRequest(string Email, string Password);
    
    public record AuthResponse
    {
        public Guid Id { get; init; }
        public required string Email { get; init; }
        public required string FullName { get; init; }
        public required string Role { get; init; }
        public required string Token { get; init; }
    }
}
