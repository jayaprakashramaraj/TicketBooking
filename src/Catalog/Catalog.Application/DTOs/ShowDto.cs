using System;

namespace Catalog.Application.DTOs
{
    public class ShowDto
    {
        public Guid Id { get; set; }
        public required string MovieName { get; set; }
        public required string TheaterName { get; set; }
        public DateTime StartTime { get; set; }
        public decimal Price { get; set; }
    }
}
